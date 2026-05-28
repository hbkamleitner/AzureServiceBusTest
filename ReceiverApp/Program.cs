using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Hello, World Receiver App!");

// handle received messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    string body = args.Message.Body.ToString();
    Console.WriteLine($"Received: {body}");

    // complete the message. message is deleted from the queue. 
    await args.CompleteMessageAsync(args.Message);
}

// handle any errors when receiving messages
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}

var configuration = new ConfigurationBuilder()
    .AddJsonFile("app.dev.json")
    .Build();


var clientOptions = new ServiceBusClientOptions
{
    TransportType = ServiceBusTransportType.AmqpWebSockets
};  

await using var client = new ServiceBusClient(
    configuration["ServiceBus:Namespace"],
    new ClientSecretCredential(
        configuration["AzureAd:TenantId"],
        configuration["AzureAd:ClientId"],
        configuration["AzureAd:ClientSecret"]
    ),
    clientOptions
);

try
{
    await using var processor = client.CreateProcessor(configuration["ServiceBus:QueueName"]);

    processor.ProcessMessageAsync += MessageHandler;
    processor.ProcessErrorAsync += ErrorHandler; 
    await processor.StartProcessingAsync();
    
    Console.WriteLine("Message received successfully.");
    Console.ReadLine();

    await processor.StopProcessingAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Error receiving message: {ex.Message}");
}


