using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HelloSpeech
{
    public class Statement : INotifyPropertyChanged
    {
        public string SpeakerName
        {
            get {
                if (SpeakerNameReplacements.ContainsKey(SpeakerId))
                {
                    return SpeakerNameReplacements[SpeakerId].Name.TrimEnd('.');
                }
                else
                    return SpeakerId; }
        }

        private Dictionary<string, SpeakerName> SpeakerNameReplacements;

        public string GetFormatedSpeakerIdStatement() { return String.Format("{0}: {1}", _speakerId, Text); }

        public string SpeakerId
        {
            get
            {
                return _speakerId;
            }
        }
        //Unique Id by Diarization
        string _speakerId;

        public string Text { get { return _text; }
            set { 
                _text = value;
                NotifyPropertyChanged();
            }
        }
        private string _text;

        public string Color { get { return SpeakerNameReplacements[SpeakerId].SpeakerColor; }}

        public ulong Offset { get; set; }

        
        public Statement(string SpeakerId, string Text, ulong Offset, Dictionary<string, SpeakerName> SpeakerNameReplacements) {
            _speakerId = SpeakerId;
            _text = Text;
            this.Offset = Offset;
            this.SpeakerNameReplacements = SpeakerNameReplacements;

            //subscribe for speaker name replacements changes to notify GUI if a new Speaker name is available
            if(SpeakerNameReplacements.ContainsKey(_speakerId))
            {
                this.SpeakerNameReplacements[SpeakerId].PropertyChanged += SpeakerName_Updated;
            }
        }

        private void SpeakerName_Updated(object? sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("SpeakerName");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
