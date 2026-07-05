using System.Collections.ObjectModel;
using System.Windows;
using ChatApp.Client.Services;
using ChatApp.Core;

namespace ChatApp.Client.ViewModels
{
    /// <summary>View model for the main chat window.</summary>
    public class MainViewModel : ViewModelBase, IAsyncDisposable
    {
        private const string ServerBaseUrl = "http://localhost:5095/";

        private readonly ChatApiClient _api = new(ServerBaseUrl);
        private ChatConnection? _chat;
        private UserInfo? _user;

        private string _username = "";
        private string _messageText = "";
        private string _status = "";
        private bool _isJoined;
        private bool _isBusy;

        public MainViewModel()
        {
            JoinCommand = new AsyncRelayCommand(JoinAsync, () => !IsBusy && Username.Trim().Length > 0);
            SendCommand = new AsyncRelayCommand(SendAsync, () => IsJoined && MessageText.Trim().Length > 0);
        }

        public ObservableCollection<ChatMessage> Messages { get; } = new();

        public AsyncRelayCommand JoinCommand { get; }
        public AsyncRelayCommand SendCommand { get; }

        public string Username { get => _username; set => SetProperty(ref _username, value); }
        public string MessageText { get => _messageText; set => SetProperty(ref _messageText, value); }
        public string Status { get => _status; private set => SetProperty(ref _status, value); }
        public bool IsJoined { get => _isJoined; private set => SetProperty(ref _isJoined, value); }
        public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

        private async Task JoinAsync()
        {
            var username = Username.Trim();
            IsBusy = true;
            Status = "Connecting…";
            try
            {
                _user = await _api.JoinAsync(username);

                _chat = new ChatConnection(ServerBaseUrl + "hubs/chat");
                _chat.MessageReceived += m => OnUiThread(() => Messages.Add(m));
                _chat.Reconnecting += () => OnUiThread(() => Status = "Reconnecting…");
                _chat.Reconnected += () => OnUiThread(() => Status = $"Connected as {_user.Username}");
                _chat.Closed += () => OnUiThread(() => Status = "Disconnected.");

                // Load history before connecting
                foreach (var message in await _api.GetRecentMessagesAsync())
                    Messages.Add(message);

                await _chat.StartAsync();

                IsJoined = true;
                Status = $"Connected as {_user.Username}";
            }
            catch (Exception ex)
            {
                Status = $"Could not connect: {ex.Message}";
                if (_chat is not null)
                {
                    await _chat.DisposeAsync();
                    _chat = null;
                }
                Messages.Clear();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SendAsync()
        {
            if (_chat is null || _user is null)
                return;

            var content = MessageText.Trim();
            try
            {
                await _chat.SendMessageAsync(_user.Id, content);
                MessageText = "";
            }
            catch (Exception ex)
            {
                Status = $"Send failed: {ex.Message}";
            }
        }

        private static void OnUiThread(Action action) =>
            Application.Current.Dispatcher.Invoke(action);

        public async ValueTask DisposeAsync()
        {
            if (_chat is not null)
                await _chat.DisposeAsync();
        }
    }
}
