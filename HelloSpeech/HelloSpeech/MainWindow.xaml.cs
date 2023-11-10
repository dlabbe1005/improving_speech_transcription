using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.CoreAudioApi;
using Microsoft.Win32;
using Microsoft.CognitiveServices.Speech.Transcription;
using System.IO;
using NAudio.Wave;
using System.Diagnostics;
using static System.Windows.Forms.DataFormats;
using System.Threading.Channels;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Collections;
using Azure.AI.TextAnalytics;
using Azure;
using System.Windows.Forms.Design;
using System.Collections.ObjectModel;
using HelloSpeech.DataModel;
using System.Collections.Specialized;


namespace HelloSpeech
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {


        public MainWindow()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)lvConversation.Items).CollectionChanged += ScrollConversation_LineAdded;

        }

        private void ScrollConversation_LineAdded(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (VisualTreeHelper.GetChildrenCount(lvConversation) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(lvConversation, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }
    }
}
