﻿using System.Text.RegularExpressions;

namespace IntegrationConnectors.Bitbucket
{
    public class Author
    {
        public string Raw { get; set; }

        public User User { get; set; }

        public string UserEmail
        {
            get
            {
                // Create a pattern for a word that starts with letter "M"  
                string pattern = @"<.*>";
                // Create a Regex  
                Regex rg = new Regex(pattern);
                MatchCollection matchedAuthors = rg.Matches(Raw);
                return matchedAuthors[0].Value;
            }
        }
    }
}