using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Configuration;
using System.Data.Common;
using Discord.Commands;
using System.Data.SqlClient;

namespace CuratorBot{
    public enum AddState{
        Error,
        Double,
        Ok,
    }
    public class Program{
		public static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();
     
        private Task Log(LogMessage msg){
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private DiscordSocketClient _client;
        private CommandHandler _handler;
        private CommandService _service;

        public async Task MainAsync(){

            //Database Initialization         
            Database db = new Database(); //Automatically sets up the Database functions

            _client = new DiscordSocketClient();
            _client.Log += Log;

            //Setup the Bot Token
            var token = ConfigurationManager.AppSettings["BotToken"];

            //Start the Bot
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            //Commandhandler
            _service = new CommandService();
            _handler = new CommandHandler(_client, _service);
            await _handler.InstallCommandsAsync();

            //Youtube
            YouTubeServiceClient yt = new YouTubeServiceClient();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
    }
}
