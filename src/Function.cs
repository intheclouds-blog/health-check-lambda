using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace InTheClouds.Lambda.HealthCheck
{
    /// <summary>
    /// Represents a Lambda function that calls a configurable set of endpoints and alerts on unsuccessful responses.
    /// </summary>
    public class Function
    {
        // The length of time in milliseconds to wait for an endpoint to respond.
        private const int CancelHealthCheckAfterMS = 10000;

        /// <summary>
        /// The name of the DynamoDB table that should be used to obtain the endpoints to check.
        /// </summary>
        private const string DynamoDBKeyEndpoints = "endpoints";

        /// <summary>
        /// The name of the DynamoDB table that should be used to obtain the endpoints to check.
        /// </summary>
        private const string DynamoDBTableHealthCheckSettings = "health-check-settings";

        /// <summary>
        /// The AWS DynamoDB client used to obtain health check endpoints.
        /// </summary>
        private IAmazonDynamoDB _ddbClient;

        /// <summary>
        /// The <see cref="HttpClient"/> that should be used for endpoint health checks.
        /// </summary>
        private HttpClient _httpClient;

        /// <summary>
        /// The current version for the function.
        /// </summary>
        private string _version;

        /// <summary>
        /// Initializes a new instance of the Lambda function.
        /// </summary>
        /// <remarks>
        /// This constructor is used by Lambda to construct an instance of the function. When invoked within a Lambda environment,
        /// the AWS credentials will come from the IAM role associated with the function, and the AWS region will be set to the 
        /// region in which the Lambda function is executed. The FunctionHandler method may be invoked more than once for a single
        /// function instance, so the constructor may be used to initialize resources that can be reused across threads and function
        /// invocations.
        /// </remarks>
        public Function()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Lambda function using the specified instances.
        /// </summary>
        /// <remarks>
        /// This constructor is useful for injecting fake client instances for unit testing.
        /// </remarks>
        public Function(IAmazonDynamoDB ddbClient)
        {
            _version = GetFunctionVersion();
            Log($"Initializing function v{_version}.");

            _ddbClient = ddbClient ?? new AmazonDynamoDBClient();
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="context">The <see cref="ILambdaContext"/> for the Lambda function</param>
        /// <returns></returns>
        public async Task FunctionHandler(ILambdaContext context)
        {
            Log($"Starting health checks using function v{_version}.");

            // Get the list of endpoints to check.
            var endpoints = await GetEndpoints();

            // Execute a health check for each endpoint asynchronously.
            var tasks = new List<Task>();
            foreach (var endpoint in endpoints)
            {
                var task = PerformHealthCheckAsync(endpoint);

                tasks.Add(task);
            }

            // Wati for all health check tasks to complete.
            await Task.WhenAll(tasks);

            Log($"Health check complete.");
        }

        /// <summary>
        /// Gets the current version of the function assembly.
        /// </summary>
        /// <returns>The current file version.</returns>
        private string GetFunctionVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            return versionInfo.FileVersion;
        }

        /// <summary>
        /// Gets the endpoints to check from DynamoDB.
        /// </summary>
        /// <returns>
        /// A Task that represents the get operation. When the task completes successfully, the result is a list of endpoints.
        /// </returns>
        private async Task<List<string>> GetEndpoints()
        {
            const string keyName = "key";
            const string valueName = "value";

            var key = new Dictionary<string, AttributeValue>
            {
                { keyName, new AttributeValue(DynamoDBKeyEndpoints) }
            };

            var response = await _ddbClient.GetItemAsync(DynamoDBTableHealthCheckSettings, key);

            return response.Item[valueName].SS;
        }

        /// <summary>
        /// Writes a log message to the LambdaLogger.
        /// </summary>
        /// <param name="message">The message to log.</param>
        private void Log(string message)
        {
            LambdaLogger.Log(message + Environment.NewLine);
        }

        /// <summary>
        /// Asynchronously performs a health check of the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to check.</param>
        /// <returns>A Task that represents the health check operation.</returns>
        private async Task PerformHealthCheckAsync(string endpoint)
        {
            Log($"INFO: Checking endpoint {endpoint}");

            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(CancelHealthCheckAfterMS);

            var watch = new Stopwatch();
            HttpResponseMessage healthCheckResponse = null;
            Exception healthCheckException = null;

            try
            {
                watch.Start();

                healthCheckResponse = await _httpClient.GetAsync(endpoint, tokenSource.Token);
            }
            catch (Exception ex)
            {
                healthCheckException = ex;
            }
            finally
            {
                watch.Stop();
            }

            if (healthCheckException != null)
            {
                Log($"ERROR: Health check failed for {endpoint} after {watch.ElapsedMilliseconds:N0}ms. {healthCheckException.Message}");
            }
            else if (healthCheckResponse != null)
            {
                if (healthCheckResponse.IsSuccessStatusCode)
                {
                    Log($"INFO: Health check successful for {endpoint} in {watch.ElapsedMilliseconds:N0}ms.");
                }
                else
                {
                    Log($"ERROR: Health check failed for {endpoint} in {watch.ElapsedMilliseconds:N0}ms. Status Code: {healthCheckResponse.StatusCode}");
                }
            }
            else
            {
                Log($"ERROR: Health check failed for {endpoint} in {watch.ElapsedMilliseconds:N0}ms. No response provided.");
            }
        }
    }
}
