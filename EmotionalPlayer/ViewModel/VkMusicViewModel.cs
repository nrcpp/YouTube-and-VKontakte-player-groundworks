using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AlphaChiTech.Virtualization;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;

namespace EmotionalPlayer.ViewModel
{
    public class VkSong
    {
        public string FullTitle { get; set; }
        public string Url { get; set; }
        public int Duration { get; set; }

        VkNet.Model.Attachments.Audio VkAudio { get; set; }

        public override string ToString()
        {
            return Url;
        }
    }

    public class VkMusicSource : IPagedSourceProvider<VkSong>
    {
        private VkApi _vkApi;
        private VkNet.Model.User _vkUser;

        const ulong _appID = 5451923;                  	// ID приложения
        List<VkSong> _songsCache = null;

        #region VK API to authorize and get audio

        public void SignInViaApi(string emailOrPhone, string password)
        {
            _vkApi = new VkApi();
            //var tel = email.Substring(3, email.Length - 3 - 2);

            _vkApi.Authorize(new ApiAuthParams
            {
                ApplicationId = _appID,
                Login = emailOrPhone,
                Password = password,
                Settings = Settings.Audio | Settings.Video | Settings.Friends,
                //TwoFactorAuthorization = () => email.Substring(3, email.Length - 3 - 2)
            });

        }

        public void GetAudio()
        {
            Debug.Assert(_vkApi.IsAuthorized);

            _songsCache = _vkApi.Audio.Get(_vkApi.UserId.Value).ToList().ConvertAll<VkSong>(
                s => new VkSong()
                {
                    FullTitle = string.Format("{0} - {1} [{2}:{3}]", s.Artist, s.Title, s.Duration / 60, s.Duration % 60),
                    Url = s.Url.ToString(),
                });
        }

        #endregion


        #region IPagedSourceProvider overrides

        public PagedSourceItemsPacket<VkSong> GetItemsAt(int pageoffset, int count, bool usePlaceholder)
        {            
            return new PagedSourceItemsPacket<VkSong>()
            {
                LoadedAt = DateTime.Now,
                Items = VkMusicEngine.Instance.SongsCache.Skip(pageoffset).Take(count)
            };
        }

        public int Count
        {
            get { return VkMusicEngine.Instance.SongsCache.Count; }
        }

        public int IndexOf(VkSong item)
        {
            return VkMusicEngine.Instance.SongsCache.IndexOf(item);
        }


        /// <summary>
        /// This is a callback that runs when a Reset is called on a provider. Implementing this is also optional. 
        /// If you don’t need to do anything in particular when resets occur, you can leave this method body empty.
        /// </summary>
        /// <param name="count"></param>
        public void OnReset(int count)
        {

        }

        #endregion
    }

    class VkMusicEngine
    {
        private VkApi _vkApi;
        private VkNet.Model.User _vkUser;

        const ulong _appID = 5451923;                  	// ID приложения
        
        public static VkMusicEngine _instace = null;
        
        public static VkMusicEngine Instance
        {
            get
            {
                if (_instace == null)
                    _instace = new VkMusicEngine();
                return _instace;
            }
        }

        public List<VkSong> SongsCache
        {
            get;
            private set;
        }

        public VkMusicEngine() 
        {
            SongsCache = new List<VkSong>();
        }

        public void SignInViaApi(string emailOrPhone, string password)
        {
            _vkApi = new VkApi();
            //var tel = email.Substring(3, email.Length - 3 - 2);

            _vkApi.Authorize(new ApiAuthParams
            {
                ApplicationId = _appID,
                Login = emailOrPhone,
                Password = password,
                Settings = Settings.Audio | Settings.Video | Settings.Friends,
                //TwoFactorAuthorization = () => email.Substring(3, email.Length - 3 - 2)
            });

        }

        public void GetAudio()
        {
            Debug.Assert(_vkApi.IsAuthorized);

            SongsCache = _vkApi.Audio.Get(_vkApi.UserId.Value).ToList().ConvertAll<VkSong>(
                s => new VkSong() {
                    FullTitle = string.Format("{0} - {1} [{2}:{3}]", s.Artist, s.Title, s.Duration / 60, s.Duration % 60),
                    Url = s.Url.ToString(),
                });        
        }        
    }

    public class VkMusicViewModel
    {
        private VirtualizingObservableCollection<VkSong> _vkAudioCollectionVirtualized = null;
        
        public VirtualizingObservableCollection<VkSong> Songs
        {
            get
            {
                if (_vkAudioCollectionVirtualized == null)
                {
                    _vkAudioCollectionVirtualized = new VirtualizingObservableCollection<VkSong>(
                            new PaginationManager<VkSong>(new VkMusicSource())
                    );
                }
                return _vkAudioCollectionVirtualized;
            }
        }

        /// <summary>
        /// Call this routine inside main window ctor
        /// </summary>
        /// <param name="mainWindow"></param>
        public static void InitVirtualizationManager(Window mainWindow)
        {
            // this routine only needs to run once, so first check to make sure the
            // VirtualizationManager isn’t already initialized
            if (!VirtualizationManager.IsInitialized)
            {
                // set the VirtualizationManager’s UIThreadExcecuteAction. In this case
                // we’re using Dispatcher.Invoke to give the VirtualizationManager access
                // to the dispatcher thread, and using a DispatcherTimer to run the background
                // operations the VirtualizationManager needs to run to reclaim pages and manage memory.
                VirtualizationManager.Instance.UIThreadExcecuteAction =
                    (a) => mainWindow.Dispatcher.Invoke(a);
                new DispatcherTimer(
                    TimeSpan.FromSeconds(1),
                    DispatcherPriority.Background,
                    delegate(object s, EventArgs a)
                    {
                        VirtualizationManager.Instance.ProcessActions();
                    },
                    mainWindow.Dispatcher).Start();
            }
        }
    }
}
