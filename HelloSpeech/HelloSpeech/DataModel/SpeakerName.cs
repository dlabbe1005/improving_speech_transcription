using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HelloSpeech
{
    public class SpeakerName : INotifyPropertyChanged
    {
        private static readonly string[] colors = { "Blue", "Orange", "Red" }; 
        private static int colorIndex = 0;

        public string Name { get {
                return _name;
            } 
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }
        private string _name;
        public string SpeakerID { get { return _speakerID; }
            set
            {
                _speakerID = value;
                NotifyPropertyChanged();
            }
        }
        private string _speakerID;

        public SpeakerName(string Name, string SpeakerId) {
            _name = Name;
            _speakerID = SpeakerId;
            SpeakerColor = GetNextColor();
        }

        public string SpeakerColor { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static string GetNextColor()
        {
            string color = SpeakerName.colors[SpeakerName.colorIndex];
            SpeakerName.colorIndex = (SpeakerName.colorIndex+1 )% colors.Length;
            return color;
        }
    }
}
