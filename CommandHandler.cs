using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Net;
using Microsoft.VisualBasic;
using Discord;

namespace CuratorBot{
    public class CommandHandler{
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public CommandHandler(DiscordSocketClient client, CommandService commands){
            _commands = commands;
            _client = client;
        }

        public async Task InstallCommandsAsync(){
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }
        //Checks every Message for a Youtube Link and also checks for Commands

        private async Task HandleCommandAsync(SocketMessage messageParam){

            //Get the message
            var message = messageParam as SocketUserMessage;

            //Check if the Message is empty is from the Bot
            if (message == null || message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);

            // Looks for and Adds a youtube Link 
            await functions.addYoutube(message.Content.ToString(), _client, context, true);

            //-- Command Checkings -- //
            //Filter: Prefix !, + is a user and not a Bot
            int argPos = 0;
            
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) || message.Author.IsBot)
                return;
                
            //The Command //ADD: message = youtubelink!                 
            var result = await _commands.ExecuteAsync(  context,  argPos, null );
        }
    }
}

/*
 *
 */