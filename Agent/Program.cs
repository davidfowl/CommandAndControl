// See https://aka.ms/new-console-template for more information
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;


var connection = new HubConnectionBuilder()
                    .WithUrl("https://localhost:7177/agent")
                    .WithAutomaticReconnect()
                    .Build();

var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole();
});

var logger = loggerFactory.CreateLogger<Agent>();

var hub = new Agent(logger);

connection.On(nameof(hub.GetTemperature), hub.GetTemperature);
connection.On(nameof(hub.Shutdown), hub.Shutdown);

logger.LogInformation("Starting agent");

await connection.StartAsync();

logger.LogInformation("Agent {ConnectionId} connected. Waiting for commands.", connection.ConnectionId);

var tcs = new TaskCompletionSource();
using var reg = PosixSignalRegistration.Create(PosixSignal.SIGINT, _ => tcs.TrySetResult());
await tcs.Task;
