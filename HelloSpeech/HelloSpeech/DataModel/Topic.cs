using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HelloSpeech
{
    public class Topic
    {
        public class TalkingPoint: INotifyPropertyChanged
        {
            public string TalkingPointContent { get; set; }
            private bool _state;
            public bool State { get {
                    return _state;
                } set {
                    //Only Mark it as true, don't move back to false
                    if (value == true)
                    {
                        _state = value;
                        NotifyPropertyChanged();
                        NotifyPropertyChanged(nameof(StateColor));
                    }
                } }
            public TalkingPoint(string content, string prompt) { 
                TalkingPointContent = content; 
                ChatGPTPrompt = prompt;
                State = false;
            }
            public string ChatGPTPrompt { get; set; }

            public string StateColor
            {
                get { if (State)
                        return "Green";
                    else
                        return "Red";
                    }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public string Title { get; set; }
        public List<TalkingPoint> TalkingPoints { get; set;}
        //public readonly cgptsQuery;

        public List<string> RelatedTopics { get; set; }



        public static Topic CreateTopicInsurance()
        {
            Topic topic = new Topic();
            topic.Title = "Sign-Up for health insurance";
            topic.TalkingPoints = new List<TalkingPoint>
            {
                new TalkingPoint("Ask for full Name", "Did the Operator ask the Caller for a full name?, Only answer with 'true' or 'false'!"),
                new TalkingPoint("Ask for Telephone Number", "Did the Operator ask the Caller for a Telephone Number? Only answer with 'true' or 'false'!. No Explanation. Direct answer."),
                new TalkingPoint("Check Job", "Did the Operator check the applicant's job? Only answer with 'true' or 'false'!"),
                new TalkingPoint("Get Social Security Number", "Did the Operator get the applicant's social security number? Only answer with 'true' or 'false'!")
            };

            topic.RelatedTopics = new List<string>
            {
                "Recommend a life insurance with Contoso to the Customer",
                "Recommend upgrading the health insurance to family plan",
                "Recommend a car and health insurance combi plan for better rates"
            };

            return topic;

        }


        public static Topic CreateTopicPrescriptionRefill()
        {
            Topic topic = new Topic();
            topic.Title = "Refill prescription medicine";
            topic.TalkingPoints = new List<TalkingPoint>
            {
                new TalkingPoint("Ask for full name", "Did the Operator ask the Caller for a full name?, Only answer with 'true' or 'false'!"),
                new TalkingPoint("Check prescription history", "Did the Operator check what drug the Caller was prescribed by doctor? Only answer with 'true' or 'false'!"),
                new TalkingPoint("Confirm which drug the call is about", "Did the Operator confirm the medicine's name that the call is about? Only answer with 'true' or 'false'!"),
                new TalkingPoint("Check if an active prescription is present", "Did the Operator if there is an active prescription for the medicine by the caller's doctor? Only answer with 'true' or 'false'!")
            };

            topic.RelatedTopics = new List<string>
            {
                "Recommend our prescription renewal reminder service",
            };

            return topic;

        }

        public static Topic CreateEmpty()
        {
            Topic topic = new Topic();
            topic.Title = "Unknown";
            topic.TalkingPoints = new List<TalkingPoint>();
            
            return topic;

        }
    }
}
