using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.CognitiveServices.Speech;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace HelloSpeech.DataModel
{
    public class SpeechDataModel : INotifyPropertyChanged
    {
        readonly Dispatcher _dispatcher;

        public ObservableCollection<Statement> FullConversation { get; set; }
        public Dictionary<string, SpeakerName> SpeakerNames { get; set; }

        public string StatusBarText { get {
                return _statusBarText;
            } set { 
                _statusBarText = value;
                NotifyPropertyChanged();

            } }

        private string _statusBarText;

        private ResetableAudioStream? audioStream;

        private string _liveText = string.Empty;
        public string LiveText
        {
            get { return _liveText; } set
            {
                _liveText = value;
                NotifyPropertyChanged();
            }
        }

        private readonly List<Topic> TopicList;

        public Topic ActiveTopic
        {
            get
            {
                return _activeTopic;
            }
            set
            {
                _activeTopic = value;
                NotifyPropertyChanged();
            }
        }

        public PointCollection SentimentPoints { get; set; }

        private readonly List<DocumentSentiment> ConversationSentiments;

        private bool _audioPlayback;
        public bool AudioPlayback { get { return _audioPlayback; } 
            set{
                _audioPlayback = value;
                NotifyPropertyChanged();
            }
        }
        

        private string _speechRegion;
        private string _speechKey;
        private string _languageEndpoint;
        private string _languageKey;
        private string _openAIEndpoint;
        private string _openAIKey;
        private string _openAIDeploymentName;

        private ChatGPTClient? cgtClient;
        private AzureAiTextAnalysisClient? aaiTaClient;
        private ConversationTranscriber? conversationTranscriber;

        public string SpeechServiceRegion { get {
                return _speechRegion;
            } 
            set {
                _speechRegion = value;
                NotifyPropertyChanged();
            } 
        }

        public string SpeechServiceKey
        {
            get
            {
                return _speechKey;
            }
            set
            {
                _speechKey = value;
                NotifyPropertyChanged();
            }
        }

        public string LanguageServiceEndpoint
        {
            get
            {
                return _languageEndpoint;
            }
            set
            {
                _languageEndpoint = value;
                NotifyPropertyChanged();
            }
        }

        public string LanguageServiceKey
        {
            get
            {
                return _languageKey;
            }
            set
            {
                _languageKey = value;
                NotifyPropertyChanged();
            }
        }


        public string OpenAIServiceEndpoint
        {
            get
            {
                return _openAIEndpoint;
            }
            set
            {
                _openAIEndpoint = value;
                NotifyPropertyChanged();
            }
        }

        public string OpenAIServiceKey
        {
            get
            {
                return _openAIKey;
            }
            set
            {
                _openAIKey = value;
                NotifyPropertyChanged();
            }
        }

        public string OpenAIDeploymentName
        {
            get
            {
                return _openAIDeploymentName;
            }
            set
            {
                _openAIDeploymentName = value;
                NotifyPropertyChanged();
            }
        }




        private Topic _activeTopic ;

        public SpeechDataModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            FullConversation = new ObservableCollection<Statement>();
            SpeakerNames = new Dictionary<string, SpeakerName>();
            TopicList = new List<Topic>
            {
                Topic.CreateEmpty(),
                Topic.CreateTopicInsurance(),
                Topic.CreateTopicPrescriptionRefill()
            };

            _activeTopic = TopicList[0];
            _statusBarText = "Ready";
            SentimentPoints = new PointCollection();

            ConversationSentiments = new List<DocumentSentiment>();
            AudioPlayback = true;

            _speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY") ?? string.Empty;
            _speechRegion = Environment.GetEnvironmentVariable("SPEECH_ENDPOINT") ?? string.Empty;

            _languageEndpoint = Environment.GetEnvironmentVariable("LANGUAGE_ENDPOINT") ?? string.Empty;
            _languageKey = Environment.GetEnvironmentVariable("LANGUAGE_KEY") ?? string.Empty;



            _openAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
            _openAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? string.Empty;
            _openAIDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DeploymentName") ?? string.Empty;


        }

        public void AddSentiment(DocumentSentiment sentiment)
        {
            ConversationSentiments.Add(sentiment);
            //Transfer String statement to Sentiment Coordinates
            int yAxis = 0;
            switch (sentiment.Sentiment)
            {
                case TextSentiment.Negative:
                    yAxis = 100;
                    break;
                case TextSentiment.Positive:
                    yAxis = 10;
                    break;
                case TextSentiment.Neutral: 
                    yAxis = 50; 
                    break;
                case TextSentiment.Mixed:
                    yAxis = 70;
                    break;
            }
            System.Windows.Point point;
            PointCollection pc = SentimentPoints.Clone();
            if(pc.Count > 16) {
                pc.RemoveAt(0);
                for(int i = 0; i < pc.Count; i++)
                {
                    pc[i] = new System.Windows.Point(pc[i].X -30, pc[i].Y);
                }
            }
            if(pc.Count == 0) {
                point = new System.Windows.Point(5, yAxis);
                
            }
            else
            {
                point = new System.Windows.Point(pc[^1].X + 30, yAxis);
            }

            pc.Add(point);
            SentimentPoints = pc;


            NotifyPropertyChanged(nameof(SentimentPoints));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateTopic(string topic)
        {
            if(!int.TryParse(topic, out int id))
            {
                id = 0;
            }

            if(id >= 0 && id < TopicList.Count)
            {
                ActiveTopic = TopicList[id];
            }
            else
            {
                ActiveTopic = TopicList[0];
            }
        }

        public string GetFullConversation()
        {
            return string.Join(Environment.NewLine + Environment.NewLine, FullConversation.Select(x => x.GetFormatedSpeakerIdStatement()).ToList());

        }


        private ICommand? _clickTransscribeConversation;
        public ICommand ClickTransscribeConversation
        {
            get
            {
                if (_clickTransscribeConversation == null)
                    _clickTransscribeConversation = new CommandHandler(TransscribeAction);
                return _clickTransscribeConversation;
            }
        }

        private ICommand? _clickSaveSettings;
        public ICommand ClickSaveSettings
        {
            get { if (_clickSaveSettings == null)
                    _clickSaveSettings = new CommandHandler(SaveSettings);
            return _clickSaveSettings;
            }
        }

        public void SaveSettings()
        {
            Task.Factory.StartNew(() => Environment.SetEnvironmentVariable("SPEECH_KEY", SpeechServiceKey, EnvironmentVariableTarget.User));
            Task.Factory.StartNew(() => Environment.SetEnvironmentVariable("SPEECH_ENDPOINT", SpeechServiceRegion, EnvironmentVariableTarget.User));

            Task.Factory.StartNew(() => Environment.SetEnvironmentVariable("LANGUAGE_ENDPOINT", LanguageServiceEndpoint, EnvironmentVariableTarget.User));
            Task.Factory.StartNew(() => Environment.SetEnvironmentVariable("LANGUAGE_KEY", LanguageServiceKey, EnvironmentVariableTarget.User));

            Task.Factory.StartNew(() => Environment.SetEnvironmentVariable("AZURE_OPENAI_ENDPOINT", OpenAIServiceEndpoint, EnvironmentVariableTarget.User));
            Task.Factory.StartNew(() => Environment.SetEnvironmentVariable("AZURE_OPENAI_API_KEY", OpenAIServiceKey, EnvironmentVariableTarget.User));
            Task.Factory.StartNew(() => Environment.SetEnvironmentVariable("AZURE_OPENAI_DeploymentName", OpenAIDeploymentName, EnvironmentVariableTarget.User));
        }

        public async void TransscribeAction()
        {
            if(conversationTranscriber !=null)
            {
                await conversationTranscriber.StopTranscribingAsync();
            }
            if(audioStream != null)
            {
                audioStream.StopAudioPlayback();
                audioStream.Dispose();
            }
            FullConversation.Clear();
            SpeakerNames.Clear();
            ActiveTopic = TopicList[0];
            SentimentPoints.Clear();
            ConversationSentiments.Clear();


            //var speechConfig = SpeechConfig.FromSubscription(SpeechServiceKey, SpeechServiceRegion);
            var speechConfig = SpeechConfig.FromEndpoint(new Uri(SpeechServiceRegion), SpeechServiceKey);
            speechConfig.SpeechRecognitionLanguage = "en-US";

            cgtClient = new ChatGPTClient(OpenAIServiceEndpoint, OpenAIServiceKey, OpenAIDeploymentName);
            aaiTaClient = new AzureAiTextAnalysisClient(LanguageServiceEndpoint, LanguageServiceKey);

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".wav";
            String Filename;
            if (openFileDialog.ShowDialog() == true)
                Filename = openFileDialog.FileName;
            else
                return;

            audioStream = new ResetableAudioStream(this, Filename);
            WaveFormat fileFormat = audioStream.GetAudioProperties();

            var audioFormat = AudioStreamFormat.GetWaveFormatPCM((uint)fileFormat.SampleRate, (byte)fileFormat.BitsPerSample, (byte)fileFormat.Channels);
            //ContosoAudioStream ast = new ContosoAudioStream();


            var audioConfig = AudioConfig.FromStreamInput(audioStream, audioFormat);

            // create a conversation transcriber using audio stream input
            conversationTranscriber = new ConversationTranscriber(speechConfig, audioConfig);

            //Pretrain the Speaker Recognition so we don't have unknown speakers in the Transscript
            StatusBarText = "Training Speakers...";
            await audioStream.TrainSpeakers(conversationTranscriber);


            conversationTranscriber.Transcribing += ConversationTranscriber_Transcribing;
            conversationTranscriber.Transcribed += ConversationTranscriber_Transcribed;

            conversationTranscriber.Canceled += ConversationTranscriber_Canceled;

            conversationTranscriber.SessionStopped += ConversationTranscriber_SessionStopped;

            var conversationTaskReal = conversationTranscriber.StartTranscribingAsync();
            if (AudioPlayback)
                audioStream.StartAudioPlayback();

            StatusBarText = "Transcribing Conversation...";

            await conversationTaskReal;
        }

        private void ConversationTranscriber_SessionStopped(object? sender, SessionEventArgs e)
        {
            StatusBarText = $"Session stopped. SessionId={e.SessionId}";
        }

        private void ConversationTranscriber_Canceled(object? sender, ConversationTranscriptionCanceledEventArgs e)
        {
            StatusBarText = $"CANCELED: Reason={e.Reason}";
        }

        private async void ConversationTranscriber_Transcribed(object? sender, ConversationTranscriptionEventArgs e)
        {
            Task<DocumentSentiment> sentimentStatement = aaiTaClient!.GetSentimentAsync(e.Result.Text);

            //Check if it is a new Speaker
            if (!SpeakerNames.ContainsKey(e.Result.SpeakerId))
            {
                SpeakerNames.Add(e.Result.SpeakerId, new SpeakerName(e.Result.SpeakerId, e.Result.SpeakerId));
            }
            await _dispatcher.InvokeAsync(() => { FullConversation.Add(new Statement(e.Result.SpeakerId, e.Result.Text, e.Offset, SpeakerNames)); });
            //Dispatcher.CurrentDispatcher.InvokeAsync
            //FullConversation.Add(new Statement(e.Result.SpeakerId, e.Result.Text, e.Offset, SpeakerNames));
            //await cvAppContent.Dispatcher.InvokeAsync(() => {});
            //await lvConversation.Dispatcher.InvokeAsync(() => { fullConversationNew.Add(new Statement(e.Result.SpeakerId, e.Result.Text, "Red", e.Offset, speakerNames)); });

            Task<string> name = cgtClient!.GetSpeakerName(SpeakerNames[e.Result.SpeakerId].SpeakerID, GetFullConversation());
            Task<string> topic = cgtClient.GetPrompt(GetFullConversation(), ChatGPTClient.ChatGPTPromptTopicNumbers, "You are an AI System analyzing a Call Center Conversation. Only answer with the number of the correct option.");
            await topic;
            UpdateTopic(topic.Result);

            Task tCheckTopic = cgtClient.CheckTopicTalkingPoints(GetFullConversation(), ActiveTopic);
            await name;
            SpeakerNames[e.Result.SpeakerId].Name = name.Result;

            await topic;
            UpdateTopic(topic.Result);

            await sentimentStatement;

            await _dispatcher.InvokeAsync(() =>
            {
                AddSentiment(sentimentStatement.Result);
            });

        }

        private void ConversationTranscriber_Transcribing(object? sender, ConversationTranscriptionEventArgs e)
        {
            LiveText = e.Result.Text;
        }

        private class CommandHandler : ICommand
        {
            private readonly Action _action;
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public CommandHandler(Action action) {
                _action = action;
            }

            public void Execute(object? parameter)
            {
                _action();
               
            }
        }

    }
}
        
    

