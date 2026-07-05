using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using ChatApp.Core;

namespace ChatApp.Client.Services
{
    /// <summary>Talks to the server's REST endpoints (users + message history).</summary>
    public class ChatApiClient
    {
        private readonly HttpClient _http;

        public ChatApiClient(string baseUrl)
        {
            _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        /// <summary>Creates the user, or reuses the existing one if the username is already taken.</summary>
        public async Task<UserInfo> JoinAsync(string username)
        {
            var response = await _http.PostAsJsonAsync("api/users", new { Username = username });

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var users = await _http.GetFromJsonAsync<List<UserInfo>>("api/users") ?? [];
                var existing = users.FirstOrDefault(
                    u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
                if (existing is not null)
                    return existing;
            }

            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<UserInfo>())!;
        }

        public async Task<List<ChatMessage>> GetRecentMessagesAsync(int count = 50)
        {
            return await _http.GetFromJsonAsync<List<ChatMessage>>($"api/messages?count={count}") ?? [];
        }
    }

    public record UserInfo(int Id, string Username);
}
