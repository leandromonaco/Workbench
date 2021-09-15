# Development Strategy

## Table of contents
- [Introduction](#introduction)
- [General](#general)
- [Guidelines](#guidelines)
- [Conventions](#conventions)
- [Best Practices](#best-practices)
- [Nuget Packages](#nuget-packages)

## Introduction

This document is a set of upfront decisions that allows the team to come up with an effective set of dos, don’ts and hows regarding 
the future application design, development and deployment, and move consistently through each step of a development project. 

## General

- Use GitHub folder structure (src, doc, test, build, tool)
- Use sqlproj project file for SQL databases
- Use the latest .NET LTS version. See [.NET Roadmap](https://github.com/dotnet/core/blob/main/roadmap.md)

## Guidelines

Area | Guideline
------------ | -------------
Branching | See [GitHub Flow](https://guides.github.com/introduction/flow/)
Code Review | See [Guidelines](https://google.github.io/eng-practices/review/)
Changelog | See [Guidelines](https://keepachangelog.com/en/1.0.0/)
Clean Architecture | See [Guidelines](https://github.com/jasontaylordev/CleanArchitecture)

## Conventions
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Azure Resources Naming Conventions](https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming)

## Best Practices

- [Asynchronous Programming](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md)
- [ASP.NET Core](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AspNetCoreGuidance.md)

## Nuget Packages

Area | Packages
------------ | -------------
Testing | - [Specflow](https://specflow.org/tools/specflow/) <br> - [xUnit](https://xunit.net/) <br> - [nSubstitute](https://nsubstitute.github.io/) <br> - [Fluent Assertions](https://fluentassertions.com/) <br> - [Fluent Validations](https://fluentvalidation.net/) <br> - [AutoFixture](https://autofixture.github.io/) <br> - [BenchmarkDotNet](https://benchmarkdotnet.org/) <br> - [Coverlet](https://dotnetfoundation.org/projects/coverlet)
Logging | - [Serilog](https://serilog.net/) <br> - [SEQ](https://datalust.co/seq)
Worker Service  | - [Quartz.NET](https://www.quartz-scheduler.net/)
Versioning | - [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) <br> - [ASP.NET API Versioning](https://github.com/dotnet/aspnet-api-versioning)
Build | - [Nuke](https://nuke.build/)
