using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EmotionalPlayer.Video;
using EmotionalPlayer.ViewModel;
using Google.Apis.YouTube.v3.Data;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using YoutubeExtractor;

namespace EmotionalPlayer.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ulong appID = 0; // TODO: SET YOUR VK APPID                  	// ID приложения
        string email = "<SET USER VK EMAIL or PHONE>";        	// email или телефон
        string pass = "<SET USER VK PASSWORD>";              	// пароль для авторизации
        Settings scope = Settings.Audio | Settings.Video | Settings.Friends;

        public VkMusicViewModel VkMusic
        {
            get;
            set;
        }


        public MainWindow()
        {
            InitializeComponent();

            VkMusic = new VkMusicViewModel();
            
            DataContext = this;
            Loaded += MainWindow_Loaded;
            
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            VkMusicViewModel.InitVirtualizationManager(this);

            var task = new Task(() =>
            {
                VkMusicEngine.Instance.SignInViaApi(email, pass);
                VkMusicEngine.Instance.GetAudio();
            });
            task.ContinueWith(
                 (t) => {
                     _songList.Dispatcher.BeginInvoke(new Action(() =>
                        _songList.ItemsSource = VkMusic.Songs));                 
                 }
             );

            try
            {
                task.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            //var vk = new VkApi();
            //var tel = email.Substring(3, email.Length - 3 - 2);

            //vk.Authorize(new ApiAuthParams
            //{
            //    ApplicationId = appID,
            //    Login = email,
            //    Password = pass,
            //    Settings = scope,
            //    //TwoFactorAuthorization = () => email.Substring(3, email.Length - 3 - 2)
            //});

            ////User vkUser;
            //_songList.ItemsSource = vk.Audio.Get(vk.UserId.Value).ToList().ConvertAll<VkSong>(
            //    s => new VkSong() {
            //        FullTitle = string.Format("{0} - {1} [{2}:{3}]", s.Artist, s.Title, s.Duration / 60, s.Duration % 60),
            //        Url = s.Url.ToString(),
            //    });        
            
        }


        private void _songList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_songList.SelectedItem == null)
                return;

            string mp3url = _songList.SelectedItem.ToString();
            int ix = mp3url.IndexOf('?');
            if (ix > 0)
                mp3url = mp3url.Remove(ix);

            // play the song            
            PlaySong(mp3url);
            
        }

        private void PlaySong(string url)
        {
            //_audioPlayer.AudioStream.StopCommand.Execute(null);
            //_audioPlayer.AudioStream.InputPath = url;
            //_audioPlayer.AudioStream.PlayCommand.Execute(null);
            _videoPlayer.Source = new System.Uri(url);
            _videoPlayer.Play();
        }

        #region Video Player
        YouTubeSearch _youtubeSearch = new YouTubeSearch();

        private string ExtractVideoUrl(string youtubeUrl)
        {
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(youtubeUrl, false);
            var video = videoInfos.First();
            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            return video.DownloadUrl;       // videoInfos.First().DownloadUrl;
        }

        public void PlayVideo(string videoId)
        {                        
            //var url = string.Format("https://www.youtube.com/embed/{0}?autoplay=1&showinfo=0&controls=0&enablejsapi=1", videoId);
            var url = string.Format("https://www.youtube.com/watch?v={0}", videoId);
            var videoUrl = ExtractVideoUrl(url);

            _geckoBrowser.Navigate(videoUrl);
        }

        private void EvalJsFromFile(string fileName)
        {            
            string jCode = File.ReadAllText(fileName);

            EvalJsString(jCode);
        }

        private void EvalJsString(string jCode)
        {
            _webBrowser.InvokeScript("eval", new object[] { jCode });            
        }

        private void InjectJs(string js)
        {
            dynamic document = this._webBrowser.Document;
            dynamic head = document.GetElementsByTagName("head")[0];
            dynamic body = document.body;
            dynamic scriptEl = document.CreateElement("script");
            scriptEl.text = js;
            head.AppendChild(scriptEl);            
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            var result = _youtubeSearch.Search(_textBoxSearch.Text);
            _searchResultListBox.ItemsSource = result.Videos;
            _searchResultListBox.DisplayMemberPath = "Snippet.Title";
        }

        private void _searchResultListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_searchResultListBox.SelectedItem == null)
                return;

            SearchResult item = (SearchResult)_searchResultListBox.SelectedItem;
            PlayVideo(item.Id.VideoId);
        }
        
        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            PlayVideo("DfBD1xS5hJ0");
        }


        #endregion

    }
}
