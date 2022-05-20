using Google.Apis.YouTube.v3.Data;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CuratorBot
{
    public class Database{
       

    public Database(){

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            /*Create the Channel Table
            executeSQL("CREATE TABLE Channel (" +
                        "ServerId varchar(50),"+
                        "ChannelId varchar(50)," +
                        "playlist varchar(50)," +
                        "PRIMARY KEY (ServerId, ChannelId)," +


            */
        }

        //Internal functions, so I dont have to create a Database object to call these functions.

        //Get the Channels linked to a Playlist with the ServerID
        internal static List<string> GetChannels(string serverId){
            List<string> channels = new List<string>();
            using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.AppSettings["connectionString"]))
            using (MySqlCommand command = new MySqlCommand("Select * From Channel where ServerId = " + serverId, connection)){
                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader()){
                    while (reader.Read()){
                        channels.Add(reader["ChannelId"].ToString());
                    }
                }               
                return channels;
            }
        }
        internal static async Task<string> GetChannelPlaylist(string channelId) {
            string playlist = string.Empty;
            await Task.Run(() =>
            {
                using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.AppSettings["connectionString"]))
                using (MySqlCommand command = new MySqlCommand("Select * From Channel where channelId = " + channelId, connection))
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            playlist = reader["playlist"].ToString();
                        }
                    }

                }
            });
            return playlist;
        } 
        internal static string[] GetChannelPlaylists(string channelId){
            string[] playlist = new string[20];
            int index = 0;
            using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.AppSettings["connectionString"]))
            using (MySqlCommand command = new MySqlCommand("Select * From Channel where channelId = " + channelId, connection)){
                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader()){
                    while (reader.Read()){                   
                        playlist[index] = reader["playlist"].ToString();
                        index++;
                    }
                }
                Array.Resize(ref(playlist), index);
                return playlist;
            }
        }
        //Execute an SQL Command
        internal static void ExecuteSQL(string str){
            using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.AppSettings["connectionString"]))
            using (MySqlCommand command = new MySqlCommand(str, connection)){
                connection.Open();
                try { 
                    command.ExecuteNonQuery();
                }catch(Exception e){ 
                    Console.WriteLine("SQL Error: executeSQL -> " + e.Message);
                }
            }           
        }  
        //Insert into Table
        internal static bool InsertChannel(string serverId, string channelId, string playlist){
            string query = "INSERT INTO Channel(ServerId, ChannelId, playlist) " +
                                        "VALUES(@ServerId, @ChannelId, @playlist)";
            using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.AppSettings["connectionString"]))
            using (MySqlCommand command = new MySqlCommand(query, connection)){
                connection.Open();
                try
                {
                    command.Parameters.AddWithValue("@ServerId", serverId);
                    command.Parameters.AddWithValue("@ChannelId", channelId);
                    command.Parameters.AddWithValue("@playlist", playlist);
                    command.ExecuteNonQuery();
                    return true;
                }catch(Exception e){ 
                    Console.WriteLine("SQL Error: insertChannel 02 -> " + e.Message);
                    return false;
                }
            }           
        }
        internal static bool DeletePlaylist(string channelId)
        {
            using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.AppSettings["connectionString"]))
            using (MySqlCommand command = new MySqlCommand("DELETE FROM Channel where channelId = " + channelId, connection))
            {
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        internal static bool DeletePlaylist(string channelId, string playlist)
        {
            using (MySqlConnection connection = new MySqlConnection(ConfigurationManager.AppSettings["connectionString"]))
            using (MySqlCommand command = new MySqlCommand("DELETE FROM Channel where channelId = " + channelId + " AND playlist = " + playlist , connection))
            {
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

    }
}
