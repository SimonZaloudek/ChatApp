using ChatApp.Core;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Client.Services
{
    /// <summary>Wraps the SignalR connection to the server's ChatHub.</summary>
    public class ChatConnection : IAsyncDisposable
    {
        private readonly HubConnection _connection;

        public event Action<ChatMessage>? MessageReceived;
        public event Action? Reconnecting;
        public event Action? Reconnected;
        public event Action? Closed;

        public ChatConnection(string hubUrl)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _connection.On<ChatMessage>("ReceiveMessage", m => MessageReceived?.Invoke(m));
            _connection.Reconnecting += _ => { Reconnecting?.Invoke(); return Task.CompletedTask; };
            _connection.Reconnected += _ => { Reconnected?.Invoke(); return Task.CompletedTask; };
            _connection.Closed += _ => { Closed?.Invoke(); return Task.CompletedTask; };
        }

        public Task StartAsync() => _connection.StartAsync();

        public Task SendMessageAsync(int userId, string content) =>
            _connection.InvokeAsync("SendMessage", userId, content);

        public ValueTask DisposeAsync() => _connection.DisposeAsync();
    }
}
