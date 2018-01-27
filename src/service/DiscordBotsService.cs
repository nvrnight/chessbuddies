using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChessBuddies
{
    public interface IDiscordBotsService
    {
        Task UpdateStats(int serverCount);
    }
    public class DiscordBotsService : IDiscordBotsService
    {
        private readonly string _apiKey;
        private readonly string _botId;
        public DiscordBotsService(string apiKey, string botId)
        {
            _botId = botId;
            _apiKey = apiKey;

        }

        public async Task UpdateStats(int serverCount)
        {
            await Task.Run(() =>
            {
                var wc = new WebClient();
                wc.Headers.Add("Authorization", _apiKey);
                wc.Headers.Add("Content-Type", "application/json");
                var result = wc.UploadString(new Uri($"https://discordbots.org/api/bots/{_botId}/stats"), "POST", JsonConvert.SerializeObject(new { server_count = serverCount }));
            });
        }
    }
}