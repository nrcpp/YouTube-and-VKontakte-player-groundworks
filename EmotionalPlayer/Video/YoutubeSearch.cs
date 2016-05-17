using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace EmotionalPlayer.Video
{
    internal class YouTubeSearchResults
    {
        public List<SearchResult> Videos
        {
            get;
            set;
        }

        public List<SearchResult> Channels
        {
            get;
            set;
        }

        public List<SearchResult> Playlists
        {
            get;
            set;
        }


        public YouTubeSearchResults()
        {
            Videos = new List<SearchResult>();
            Channels = new List<SearchResult>();
            Playlists = new List<SearchResult>();
        }
    }

    internal class YouTubeSearchException : Exception
    {
        public YouTubeSearchException(string message, AggregateException ex)
            : base(message, ex)
        {
        }
    }


    /// <summary>
    /// YouTube Data API v3 sample: search by keyword.
    /// Relies on the Google APIs Client Library for .NET, v1.7.0 or higher.
    /// See https://code.google.com/p/google-api-dotnet-client/wiki/GettingStarted
    ///
    /// Set ApiKey to the API key value from the APIs & auth > Registered apps tab of
    ///   https://cloud.google.com/console
    /// Please ensure that you have enabled the YouTube Data API for your project.
    /// </summary>
    internal class YouTubeSearch
    {
        readonly YouTubeService _youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = "<PUT HERE YOUR YOUTUBE API KEY>",
            ApplicationName = "<YOUR APP NAME>"
        });
        
        public int MaxResults
        {
            get;
            set;
        }

        public YouTubeSearch()
        {
            MaxResults = 50;
        }

        //
        public YouTubeSearchResults Search(string query)
        {            
            try
            {
                return RunSearch(query);
            }
            catch (AggregateException ex)
            {
                string message = "";
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("Error: " + e.Message);
                    message += e.Message + "\r\n";
                }

                throw new YouTubeSearchException(message, ex);
            }         
        }

        private YouTubeSearchResults RunSearch(string query)
        {            
            var searchListRequest = _youtubeService.Search.List("snippet");
            searchListRequest.Q = query;           
            searchListRequest.MaxResults = this.MaxResults;
            
            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = searchListRequest.Execute();   // ExecuteAsync();
            var result = new YouTubeSearchResults();
            
            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":                        
                        result.Videos.Add(searchResult);                        
                        break;

                    case "youtube#channel":
                        result.Channels.Add(searchResult);
                        break;

                    case "youtube#playlist":
                        result.Playlists.Add(searchResult);
                        break;
                }
            }

            return result;
        }
    }
}
