// See https://aka.ms/new-console-template for more information

using Greeter;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5001"); 
var client = new GreetingsManager.GreetingsManagerClient(channel);

var response = await client.GenerateGreetingAsync(new GreetingRequest(){ Name = "Grpc" });

Console.WriteLine(response.GreetingMessage);

