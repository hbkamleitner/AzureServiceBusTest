using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Hello, World!");


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
    await using var sender = client.CreateSender(configuration["ServiceBus:QueueName"]);

    var message = new ServiceBusMessage($"Hello again, Service Bus! {DateTime.Now}");

    await sender.SendMessageAsync(message);

    Console.WriteLine("Message sent successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error sending message: {ex.Message}");
}


