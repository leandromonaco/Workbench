using CommandLine;
using Hangfire;
using Hangfire.MemoryStorage;
using IntegrationConnectors.Common;
using JiraReporting.Model;
using JiraReporting.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JiraReporting
{
    class Program
    {
        static void Main(string[] args)
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();

            Parser.Default.ParseArguments<Options>(args)
               .WithParsed(o =>
               {
                   //ExecuteJob(o.JiraEndpoint, o.JiraProject, o.JiraUsername, o.JiraAuthenticationToken, o.PowerBiDatasetEndpoint);
                   ExecuteJob(o.JiraEndpoint, o.JQL, o.JiraUsername, o.JiraAuthenticationToken);
                   //RecurringJob.AddOrUpdate("JiraReportJob",
                   //                        () => ExecuteJob(o.JiraEndpoint, o.JiraProject, o.JiraUsername, o.JiraAuthenticationToken, o.PowerBiDatasetEndpoint),
                   //                        Cron.Hourly);
               });

            using var server = new BackgroundJobServer();
            Console.ReadLine();
        }

        public static void ExecuteJob(string jiraEndpoint, string jql, string jiraUsername, string jiraAuthenticationToken)
        {
            Console.WriteLine($"Jira Report started {DateTime.Now}");

            var dateFormat = "yyyy-MM-dd";

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{jiraUsername}:{jiraAuthenticationToken}"));

            HttpConnector httpConnector = new("", credentials, AuthenticationType.Basic);

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                //MaxDepth = 1
            };


            //Checkpoint Backlog
            List<BacklogItem> checkpointBacklog = null;
            var checkpointDate = DateTime.Now.AddDays(-7).ToString(dateFormat);
            var checkpointFile = $"{Environment.CurrentDirectory}\\report_{checkpointDate}.json";

            if (File.Exists(checkpointFile))
            {
                string checkpointBacklogJson = File.ReadAllText(checkpointFile);
                checkpointBacklog = JsonSerializer.Deserialize<List<BacklogItem>>(checkpointBacklogJson, jsonSerializerOptions);
            }

            //Latest Backlog

            var latestBacklog = new List<BacklogItem>();

            var increment = 100;
            var startAt = 0;
            var finishAt = 1;

            while (startAt <= finishAt)
            {
                var query = $@"{{
                                ""jql"": ""{jql}"",
                                ""maxResults"": {increment},
                                ""startAt"": {startAt}
                            }}";
           
                var response = httpConnector.PostAsync($"{jiraEndpoint}/rest/api/2/search", query).Result;
                var jqlQueryResult = JsonSerializer.Deserialize<JqlQueryResult>(response, jsonSerializerOptions);

                Console.WriteLine($"Processing {startAt} of {jqlQueryResult.Total} {DateTime.Now}");

                startAt += increment;
                finishAt = jqlQueryResult.Total - 1;

                foreach (var issue in jqlQueryResult.Issues)
                {
                    var issueType = issue.Fields.IssueType.Name;

                    var row = new BacklogItem
                    {
                        Date = DateTime.Now.Date,
                        Sprint = issue.Fields.Sprints?.OrderByDescending(s => s.StartDate).FirstOrDefault().Name,
                        IssueId = issue.Key,
                        IssueTitle = issue.Fields.Summary,
                        EpicId = issue.Fields.Parent?.Key,
                        EpicTitle = issue.Fields.Parent?.Fields.Summary,
                        IssueType = issueType,
                        Priority = issue.Fields.Priority?.Value,
                        Status = issue.Fields.Status.Name,
                        Points = Convert.ToInt32(issue.Fields.Points),
                        AssignedTo = issue.Fields.Assignee == null ? "Unassigned" : issue.Fields.Assignee.DisplayName
                    };

                    latestBacklog.Add(row);
                }
            }

            List<BacklogItem> changedStories = new();
            List<BacklogItem> changedBugs = new();
            List<BacklogItem> changedRaids = new();

            if (checkpointBacklog != null)
            {
                var newItems = latestBacklog.Where(pbi => !checkpointBacklog.Exists(cpbi => cpbi.IssueId.Equals(pbi.IssueId))).ToList();
                var newStories = newItems.Where(i => i.IssueType.Equals("Story")).ToList();
                var newBugs = newItems.Where(i => i.IssueType.Equals("Bug")).ToList();
                var newRaids = newItems.Where(i => i.IssueType.Contains("RAID")).ToList();

                var changedItems = latestBacklog.Where(pbi => checkpointBacklog.Exists(cpbi => cpbi.IssueId.Equals(pbi.IssueId)) &&
                                                              checkpointBacklog.Count(cpbi => !cpbi.Status.Equals(pbi.Status)) > 0).ToList();
                changedStories = changedItems.Where(i => i.IssueType.Equals("Story")).ToList();
                changedBugs = changedItems.Where(i => i.IssueType.Equals("Bug")).ToList();
                changedRaids = changedItems.Where(i => i.IssueType.Contains("RAID") || 
                                                       i.IssueType.Contains("Dependency") || 
                                                       i.IssueType.Contains("Impediment") || 
                                                       i.IssueType.Contains("Risk") || 
                                                       i.IssueType.Contains("Assumption")).ToList();
            }


            var outputFile = $"{Environment.CurrentDirectory}\\report_{DateTime.Now.Date.ToString(dateFormat)}";

            ExcelHelper.Export(latestBacklog, null, $"{outputFile}.xlsx", jiraEndpoint);
            //ExcelHelper.Export(changedStories, null, $"ChangedStories.xlsx", jiraEndpoint);
            //ExcelHelper.Export(changedBugs, null, $"ChangedBugs.xlsx", jiraEndpoint);
            //ExcelHelper.Export(changedRaids, null, $"ChangedRaids.xlsx", jiraEndpoint);

            File.WriteAllText($"{outputFile}.json", JsonSerializer.Serialize(latestBacklog));

            //var result = httpConnector.PostAsync(powerBiDatasetEndpoint, JsonSerializer.Serialize(latestBacklog)).Result;

            Console.WriteLine($"Jira Report stopped {DateTime.Now}");
        }
    }
}
