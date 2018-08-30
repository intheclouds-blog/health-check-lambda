# csharp-lambda-seed

Simple template project for creating an AWS Lambda function in C# with xUnit&#46;net unit tests.

### Features

* Flatter directory structure than the AWS supplied Lambda templates
* Simple .gitignore appropriate for C# projects

### Getting Started

1. Fork or clone the repository. Alternatively, just download the code and copy it into an existing repository.

2. Find and replace all references to `InTheClouds` and `Seed` with names appropriate for the Lambda function.

    *NOTE*: You might get errors during the rename process due to references between the test and src projects.

3. Restore dependencies and ensure the project builds:

    `dotnet build ./src`

4. Ensure the xUnit&#46;net tests run successfully:

    `dotnet test ./test`

### Dev Dependencies

* .NET Core 2.1
* Amazon.Lambda.Core 1.0.0
* Amazon.Lambda.Serialization.Json 1.3.0
* Amazon.Lambda.Tools 2.2.0
* Newtonsoft.Json 11.0.2

### Test Dependencies

* xUnit&#46;net 2.3.1
* Amazon.Lambda.TestUtilities 1.0.0
* Microsoft.NET.Test.Sdk 15.5.0