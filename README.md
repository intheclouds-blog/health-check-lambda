# health-check-lambda

AWS Lambda function that performs health checks of a configurable set of HTTP endpoints.

See the blog post linked below for more information about this function:

https://www.intheclouds.blog/2018/09/03/endpoint-health-monitoring/

### Features

* Asynchronously checks a set of HTTP endpoints
* Reads endpoints from a DynamoDB table
* Sends notifications through SNS if health checks fail

### Getting Started

1. Install .NET Core 2.1 (https://www.microsoft.com/net/download/dotnet-core/2.1). Ensure the correct version is in your path by running the following from the Terminal (macOS) or Command Prompt (Windows). The version should be at least v2.1.401.

    ```
    dotnet --version
    ```

2. Clone this repo.

    ```
    git clone https://github.com/intheclouds-blog/health-check-lambda.git
    ```

3. Run the tests to download dependencies, build, and test the code:

    ```
    cd health-check-lambda
    dotnet test ./test
    ```

### Manual Deployment

The health-check-lambda Lambda function can be manually deployed using the `.NET Core CLI` and `Amazon Lambda Tools for .NET Core applications` using a command like the following:

```
cd src
dotnet lambda deploy-function -fn health-check-lambda -frole health-check-lambda
```

### Manual Invocation

The health-check-lambda Lambda function can be manually invoked using the `.NET Core CLI` and `Amazon Lambda Tools for .NET Core applications` using a command like the following:

```
cd src
dotnet lambda invoke-function -fn health-check-lambda
```

### Dev Dependencies

* .NET Core 2.1
* Amazon.Lambda.Core 1.0.0
* Amazon.Lambda.Serialization.Json 1.3.0
* Amazon.Lambda.Tools 2.2.0
* Newtonsoft.Json 11.0.2

### Test Dependencies

* xUnit.net 2.3.1
* Amazon.Lambda.TestUtilities 1.0.0
* Microsoft.NET.Test.Sdk 15.5.0
* FakeItEasy 4.8.0