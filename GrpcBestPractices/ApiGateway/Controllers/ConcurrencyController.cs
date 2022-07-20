using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Performance;
using Monitor = Performance.Monitor;

namespace ApiGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConcurrencyController : ControllerBase
    {
        private readonly string _serverUrl;
        
        public ConcurrencyController(IConfiguration configuration)
        {
            _serverUrl = configuration.GetValue<string>("ServerUrl");
        }
        
        [HttpGet("single-connection/{count}")]
        public ResponseModel GetDataFromSingleConnection(int count)
        {
            using var channel = GrpcChannel.ForAddress(_serverUrl);
           
            var response = new ResponseModel();
            var stopwatch = Stopwatch.StartNew();
            var concurrentJobs = new List<Task>();

            for (int i = 0; i < count; i++)
            {
                var client = new Monitor.MonitorClient(channel);
                concurrentJobs.Add(Task.Run(() =>
                {
                    client.GetPerformance(new PerformanceStatusRequest()
                    {
                        ClientName = $"Client {i + 1}"
                    });
                }));
            }

            Task.WaitAll(concurrentJobs.ToArray());
            response.RequestProcessingTime = stopwatch.ElapsedMilliseconds;
            return response;
        }
        
        [HttpGet("multiple-connection/{count}")]
        public ResponseModel GetDataUsingMultipleConnection(int count)
        {
            using var channel = GrpcChannel.ForAddress(_serverUrl, new GrpcChannelOptions()
            {
                HttpHandler = new SocketsHttpHandler()
                {
                    EnableMultipleHttp2Connections = true
                }
            });
           
            var response = new ResponseModel();
            var stopwatch = Stopwatch.StartNew();
            var concurrentJobs = new List<Task>();

            for (int i = 0; i < count; i++)
            {
                var client = new Monitor.MonitorClient(channel);
                concurrentJobs.Add(Task.Run(() =>
                {
                    client.GetPerformance(new PerformanceStatusRequest()
                    {
                        ClientName = $"Client {i + 1}"
                    });
                }));
            }

            Task.WaitAll(concurrentJobs.ToArray());
            response.RequestProcessingTime = stopwatch.ElapsedMilliseconds;
            return response;
        }
    }
}
