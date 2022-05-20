using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Discord;

namespace CuratorBot{
    class functions
    {

        public static int CountStringOccurrences(string text, string pattern)
        {
            //Loop through all instances of the string.
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }
        internal static int ExtractMultiYTId(string message, ref string[] ids)
        {

            string msg = message;

            //Variables for the 2 Youtube Formats
            string ytFormat1 = "https://www.youtube.com/watch?v=";
            string ytFormat2 = "https://youtu.be/";

            int count = CountStringOccurrences(msg, ytFormat1) + CountStringOccurrences(msg, ytFormat2);
            ids = new string[count];
            for (int i = 0; i < count; i++)
            {
                //Check if it contains a link
                if (!msg.Contains(ytFormat1) && !msg.Contains(ytFormat2))
                    return 0;

                //Differentiate between the short and long URL (Not the most ideal way to do this but it works without issues)
                if (msg.Contains(ytFormat1) && msg.Contains(ytFormat2))
                {
                    if (msg.IndexOf(ytFormat1) < msg.IndexOf(ytFormat2))
                    {
                        msg = msg.Substring(msg.IndexOf(ytFormat1));
                    }
                    else
                    {
                        msg = msg.Substring(msg.IndexOf(ytFormat2));
                    }
                }
                else
                {
                    if (msg.Contains(ytFormat1))
                    {
                        msg = msg.Substring(msg.IndexOf(ytFormat1));
                    }
                    if (msg.Contains(ytFormat2))
                    {
                        msg = msg.Substring(msg.IndexOf(ytFormat2));
                    }
                }

                //Cut everything behind the link
                string rest = string.Empty;
                if (msg.Contains(' '))
                {
                    rest = msg.Substring(msg.IndexOf(' '));
                    msg = msg.Substring(0, msg.IndexOf(' '));
                }

                //Get the Link id, after =
                if (msg.Contains('='))
                    msg = ids[i] = msg.Substring(msg.IndexOf('=') + 1);

                if (msg.Contains(".be/"))
                    msg = ids[i] = msg.Substring(msg.IndexOf(".be/") + 4);

                //Cut everything after a "=" or "&" if it exists (like if a Link is copied from playlist)
                if (ids[i].Contains('='))
                    ids[i] = msg.Substring(0, ids[i].IndexOf('='));
                if (ids[i].Contains('&'))
                    ids[i] = ids[i].Substring(0, msg.IndexOf('&'));

                msg = rest;
            }
            return count;
        }


        internal static async Task<bool> addYoutube(string message, DiscordSocketClient _client, ICommandContext context, bool toLog)
        {
            //Return Youtube 
            string[] ids = new string[0];
            int count = functions.ExtractMultiYTId(message, ref ids);
            bool isSuccess = false;
            //var context = new SocketCommandContext(_client, message);
            //If a Youtube Link has been found continue with the Adding to Playlist process
            if (count != 0)
            {
                await Task.Run(async () =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        var channelId = context.Channel.Id.ToString();
                        var serverId = context.Guild.Id.ToString();

                        //Get a list of Channels from the ServerID, check if the Channel which the Link was posted on is Linked to a Playlist
                        if (Database.GetChannels(serverId).Contains(channelId))
                        {
                            var chnl = _client.GetChannel(Convert.ToUInt64(channelId)) as IMessageChannel;

                            // Supports Multiple Playlists linked to a single channel
                            string[] playlists = Database.GetChannelPlaylists(context.Channel.Id.ToString());
                            for (int j = 0; j < playlists.Length; j++)
                            {
                                string errorInfo = "";
                                //AddSongToPlaylist returns AddState, which returns different Statuses: Ok, Double or Error
                                switch (YouTubeServiceClient.AddSongToPlaylist(ids[i], playlists[j], ref(errorInfo)))
                                {
                                    case AddState.Ok:
                                        await context.Message.AddReactionAsync(new Discord.Emoji("✔️"));
                                        isSuccess = true;
                                        break;
                                    case AddState.Double:
                                        await context.Message.AddReactionAsync(new Discord.Emoji("✖️"));
                                        if (toLog)
                                            await chnl.SendMessageAsync(errorInfo);
                                        break;
                                    case AddState.Error:
                                        await context.Message.AddReactionAsync(new Discord.Emoji("✖️"));
                                        if (toLog)
                                        {
                                            if (errorInfo != null) ;
                                                // await chnl.SendMessageAsync(errorInfo);
                                        }
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Channel not Linked to a Playlist!");
                        }
                    }

                });
            }
            return isSuccess;
        }
    }
}   
    
 