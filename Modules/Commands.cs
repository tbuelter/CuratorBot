using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net.Command;

namespace CuratorBot.Modules{
    public class Commands : ModuleBase<SocketCommandContext>{

        //Link a Channel with a Playlist
        [Command("Link")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddChannel(string playlist){
            if (Database.InsertChannel(Context.Guild.Id.ToString(), Context.Channel.Id.ToString(), playlist)){
                await Context.Channel.SendMessageAsync("Successfully linked this channel with the Youtube Playlist:\n" + "https://www.youtube.com/playlist?list=" + playlist);
            }
            else{
                await Context.Channel.SendMessageAsync("Error linking Playlist to Channel");
            }    
        }   
        [Command("Unlink")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Unlink(){
            string[] playlists = Database.GetChannelPlaylists(Context.Channel.Id.ToString());
            if (playlists.Length != 0)
            {
                if (Database.DeletePlaylist(Context.Channel.Id.ToString()))
                {
                    string playlistMsg = string.Empty;
                    for (int i = 0; i < playlists.Length; i++)
                    {
                        playlistMsg += "https://www.youtube.com/playlist?list=" + playlists[i] + "\n";
                    }
                    await Context.Channel.SendMessageAsync("Successfully unlinked this channel with " + playlists.Length + " Youtube playlist/s: \n" + playlistMsg);
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Error unlinking playlist from the channel");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("This channel does not have any linked playlists!");
            }
        }   
        [Command("Unlink")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Unlink(string playlist){
            if (Database.DeletePlaylist(Context.Channel.Id.ToString(), playlist)){
          
                await Context.Channel.SendMessageAsync("Successfully unlinked this channel with the Youtube Playlist " + "https://www.youtube.com/playlist?list=" + playlist);
            }
            else{
                await Context.Channel.SendMessageAsync("Error unlinking Playlist from the Channel");
            }    
        }   
        [Command("GetLinks")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task GetLinks(){
            string[] playlists = Database.GetChannelPlaylists(Context.Channel.Id.ToString());
            string playlistMsg = string.Empty;
            for (int i = 0; i < playlists.Length; i++)
            {
                playlistMsg += "https://www.youtube.com/playlist?list=" + playlists[i] + "\n";
            }

            if(playlistMsg != string.Empty) { 
                await Context.Channel.SendMessageAsync("This channel is linked with the following Youtube Playlist/s: \n" + playlistMsg );
            }
            else{
                await Context.Channel.SendMessageAsync("No playlists linked to this Channel!");
            }    
        }

        //Command which scans the Channel chat history for Youtube Links
        [Command("History")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task History()
        {
            await Context.Channel.SendMessageAsync("Scanning through the channel messages, this might take a while!");

            // Should make a SQL COUNT query, but this function already existed and works just as good (Im lazy)
            string playlist = await Database.GetChannelPlaylist(Context.Channel.Id.ToString());
            if (playlist != string.Empty)
            {
                int count = 0;
                //var msgList = Context.Channel.GetMessagesAsync(100, CacheMode.AllowDownload);
                var messages = await Context.Channel.GetMessagesAsync(1000).FlattenAsync();
                foreach (var msg in messages)
                {
                    if (msg.Content[0] == '!' || msg.Author.IsBot || !msg.Content.Contains("youtu"))
                        continue;

                    bool success = await functions.addYoutube(msg.Content.ToString(), Context.Client, Context, false);
                    if (success)
                    {
                        Console.WriteLine(msg.Content.ToString());
                        await msg.AddReactionAsync(new Emoji("✔️"));
                        count++;
                    }
                }
                
                await Context.Channel.SendMessageAsync("Succesfully added **" + count + "** Tracks from the chat history to the playlist!");
            }
            else
            {
                await Context.Channel.SendMessageAsync("This channel is not linked to a playlist!");
            }
       
        }
    }
}
