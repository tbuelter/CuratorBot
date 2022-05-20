using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace CuratorBot{
    
    public class YouTubeServiceClient{
        private static YouTubeServiceClient instance;
        public static YouTubeServiceClient Instance{
            get{
                if (instance == null){
                    instance = new YouTubeServiceClient();
                }
                return instance;
            }
        }
        private async Task<YouTubeService> GetYouTubeService(){
            ClientSecrets Client = new ClientSecrets();
            Client.ClientId = ConfigurationManager.AppSettings["YoutubeClientId"];
            Client.ClientSecret = ConfigurationManager.AppSettings["YoutubeClientSecret"];

            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(Client, new[]  {
                        YouTubeService.Scope.Youtube
                    },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString()));
            
            var youtubeService = new YouTubeService(new BaseClientService.Initializer(){
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });
            
            return youtubeService;
        }

        internal static AddState AddSongToPlaylist( string songId, string playlistId, ref string info){
            
            AddState AddStatus = AddState.Error;
            try
            {
                YouTubeServiceClient service = new YouTubeServiceClient();
                service.AddSongToPlaylistAsync(songId, playlistId).Wait();
                Console.WriteLine("Added to Playlist");
                AddStatus = AddState.Ok;
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    //TODO: Add Logging
                    if (ex.Message.Contains("0001"))
                    {
                        info = ex.Message.Substring(ex.Message.IndexOf("0001") + 4);
                        Console.WriteLine(info);
                        AddStatus = AddState.Double;
                    }
                    else
                    {
                        Console.WriteLine(ex.Message);
                        AddStatus = AddState.Error;
                    }
                }
            }           
            return AddStatus;
        }
        private async Task AddSongToPlaylistAsync( string songId, string playlistId){
            var youtubeService = await this.GetYouTubeService();

            //Add the Link to the Playlist
            var newPlaylistItem = new PlaylistItem();
            newPlaylistItem.Snippet = new PlaylistItemSnippet();
            newPlaylistItem.Snippet.PlaylistId = playlistId;
            newPlaylistItem.Snippet.ResourceId = new ResourceId();
            newPlaylistItem.Snippet.ResourceId.Kind = "youtube#video";
            newPlaylistItem.Snippet.ResourceId.VideoId = songId;

            // -- Check if the Video is already in the Playlist -- //
            //Get the Videos in the Playlist
            PlaylistItemsResource.ListRequest list = youtubeService.PlaylistItems.List("snippet");
            list.PlaylistId = playlistId;
            list.MaxResults = 500; //Youtube API says maximum is 50, might be a problem
            PlaylistItemListResponse getListResponse = await list.ExecuteAsync();
            IList<PlaylistItem> items = getListResponse.Items;

            //Go Through the list and compare Youtube Video Ids. 
            //If the ID is found throw an exception which will be caught at the AddSongToPlaylist function
            //Exception Throws an Exception-ID at string Pos 0, "0001" stands for Double entry!
            foreach (var v in items){              
                if (newPlaylistItem.Snippet.ResourceId.VideoId == v.Snippet.ResourceId.VideoId){
                    throw new System.ArgumentException("0001 Double Entry: \"" + v.Snippet.Title + "\"\n(Playlist index: " + v.Snippet.Position + ", date added: " + v.Snippet.PublishedAt.ToString());
                }
            }

            newPlaylistItem = await youtubeService.PlaylistItems.Insert(newPlaylistItem, "snippet").ExecuteAsync();

        }
    }
}
