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
    public class PerformanceController : ControllerBase
    {
        private readonly Monitor.MonitorClient _defaultClient;
        private readonly IGrpcPerformanceClient _grpcPerformanceClient;
        private readonly string _serverUrl;
        
        public PerformanceController(Monitor.MonitorClient monitorClient, 
            IGrpcPerformanceClient grpcPerformanceClient, 
            IConfiguration configuration)
        {
            _defaultClient = monitorClient;
            _grpcPerformanceClient = grpcPerformanceClient;
            _serverUrl = configuration.GetValue<string>("ServerUrl");
        }

        [HttpGet("default-client/{count}")]
        public async Task<ResponseModel> GetPerformanceFromDefaultClient(int count)
        {
            var stopwatch = Stopwatch.StartNew();
            var responseModel = new ResponseModel();
            for (var i = 0; i < count; i++)
            {
                var response = await _defaultClient.GetPerformanceAsync(new PerformanceStatusRequest()
                {
                    ClientName = $"client {i + 1}"
                });
                responseModel.PerformanceStatuses.Add(new ResponseModel.PerformanceStatusModel()
                {
                    CpuPercentageUsage = response.CpuPercentageUsage,
                    MemoryUsage = response.MemoryUsage,
                    ActiveConnections = response.ActiveConnections,
                    ProcessesRunning = response.ProcessesRunning
                });
            }
           
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            responseModel.RequestProcessingTime = elapsedMilliseconds;
            return responseModel;
        }
        
        [HttpGet("wrapper-client/{count}")]
        public async Task<ResponseModel> GetPerformanceFromWrapperClient(int count)
        {
            var stopwatch = Stopwatch.StartNew();
            var responseModel = new ResponseModel();
            for (var i = 0; i < count; i++)
            {
                var response = await _grpcPerformanceClient.GetPerformanceStatusAsync($"client {i + 1}");
                responseModel.PerformanceStatuses.Add(new ResponseModel.PerformanceStatusModel()
                {
                    CpuPercentageUsage = response.CpuPercentageUsage,
                    MemoryUsage = response.MemoryUsage,
                    ActiveConnections = response.ActiveConnections,
                    ProcessesRunning = response.ProcessesRunning
                });
            }
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            responseModel.RequestProcessingTime = elapsedMilliseconds;
            return responseModel;
        }
        
        [HttpGet("new-client-channel/{count}")]
        public async Task<ResponseModel> GetPerformanceFromNewClient(int count)
        {
            var stopWatch = Stopwatch.StartNew();
            var response = new ResponseModel();
            for (var i = 0; i < count; i++)
            {
                var channel = GrpcChannel.ForAddress(_serverUrl);
                var client = new Monitor.MonitorClient(channel);
                var grpcResponse = await client.GetPerformanceAsync(new PerformanceStatusRequest()
                {
                    ClientName = $"client {i + 1}"
                });               
                response.PerformanceStatuses.Add(new ResponseModel.PerformanceStatusModel()
                {
                    CpuPercentageUsage = grpcResponse.CpuPercentageUsage,
                    MemoryUsage = grpcResponse.MemoryUsage,
                    ActiveConnections = grpcResponse.ActiveConnections,
                    ProcessesRunning = grpcResponse.ProcessesRunning
                });
            }
            response.RequestProcessingTime = stopWatch.ElapsedMilliseconds;
            return response;
        }

        [HttpGet("streaming-client/{count}")]
        public async Task<ResponseModel> GetPerformanceFromStreamingCall(int count)
        {
            var stopWatch = Stopwatch.StartNew();
            var response = new ResponseModel();
            var clientNames = new List<string>();

            for (int i = 0; i < count; i++)
            {
                clientNames.Add($"Client {i + 1}");
            }
            response.PerformanceStatuses.AddRange(await _grpcPerformanceClient.GetManyPerformanceStatusAsync(clientNames));
            response.RequestProcessingTime = stopWatch.ElapsedMilliseconds;
            return response;
        }
    }
}
