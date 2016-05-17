using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EmotionalPlayer.Audio
{
    /// <summary>
    /// Interaction logic for AudioStreamControl.xaml
    /// </summary>
    public partial class AudioStreamControl : UserControl
    {
        public AudioStreamControlViewModel AudioStream
        {
            get;
            private set;
        }

        public AudioStreamControl()
        {
            InitializeComponent();

            AudioStream = new AudioStreamControlViewModel();
            DataContext = AudioStream;
        }
    }
}
