using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Google.Apis.YouTube.v3.Data;

namespace EmotionalPlayer.Video
{
    /// <summary>
    /// Interaction logic for YouTubeControl.xaml
    /// </summary>
    public partial class YouTubeControl : UserControl
    {
        YouTubeSearch _youtubeSearch = new YouTubeSearch();

        public YouTubeControl()
        {
            InitializeComponent();
       
            // Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36
            Loaded += (o, e) => _webBrowser.Navigate("about:blank");
        }
        
        public void ShowVideo(string videoId)
        {
            var url = string.Format("https://www.youtube.com/embed/{0}?autoplay=1&showinfo=0&controls=0", videoId);
            string userAgentHeader = "User-Agent: Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36\r\n";

            _webBrowser.Navigate(url, null, null, userAgentHeader);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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
            ShowVideo(item.Id.VideoId);
        }
    }
}
