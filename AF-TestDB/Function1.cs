using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace AF_TestDB
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> log)
        {
            _logger = log;
        }

        [FunctionName("Function1")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "env", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The **Environment** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string serverName = "azn-sql-demo-server";
                string dbName = "azn-sqldb-demo-db";

                List<string> result = new();
                //using (SqlConnection conn = new SqlConnection($"Data Source= .\\mssql2019; Initial Catalog = TestDB; trusted_connection=true; trustServerCertificate=true"))
                using (SqlConnection conn = new SqlConnection($"Server=tcp:{serverName}.database.windows.net; Database={dbName}; Authentication=Active Directory Default;"))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM [sys].[all_objects]", conn))
                    {
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                           result.Add($"{rdr["Name"]}");
                        }
                    }
                }
              
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {

                return new OkObjectResult($"Oops, some error: {ex.Message}");
            }
        }
    }
}

