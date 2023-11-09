using HelloSpeech.DataModel;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace HelloSpeech
{
    public class ResetableAudioStream : PullAudioInputStreamCallback
    {
        private readonly string Filename;
        private readonly BinaryReader Reader;

        private bool Speaker1Identified;
        private bool Speaker2Identified;
        private bool TrainingDone;


        readonly TaskCompletionSource<int> stopTrainingSpeakers;

        private readonly SoundPlayer player;
        private readonly MemoryStream PlaybackStream;
        readonly Stopwatch stopwatch;
        private readonly SpeechDataModel DataModel;

        private readonly WaveFormat waveFormat;

        private bool isPlaying = false;


        public ResetableAudioStream(SpeechDataModel DataModel, string Filename) {
            this.Filename = Filename;
            Reader = new BinaryReader(File.OpenRead(Filename));
            Speaker1Identified = false;
            Speaker2Identified = false;
            TrainingDone = false;
            PlaybackStream = new MemoryStream();

            Reader.BaseStream.CopyTo(PlaybackStream);
            Reader.BaseStream.Position = 0;
            PlaybackStream.Position = 0;

            stopTrainingSpeakers = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            player = new SoundPlayer(PlaybackStream);
            stopwatch = new Stopwatch();
            this.DataModel = DataModel;
            using (var reader = new WaveFileReader(Filename))
            {
                this.waveFormat = reader.WaveFormat;
            }
        }



        public void StartAudioPlayback()
        {
            isPlaying = true;
            player.Play();
            stopwatch.Start();
        }

        public void StopAudioPlayback() { 
        
            player.Stop();
            stopwatch.Stop();
            stopwatch.Reset();
            isPlaying = false;
        }


        public WaveFormat GetAudioProperties()
        {
            return waveFormat;

        }


        public override int Read(byte[] dataBuffer, uint size)
        {
            if(!DataModel.AudioPlayback && isPlaying)
                StopAudioPlayback();


            if(TrainingDone && DataModel.AudioPlayback && isPlaying)
            {
                while (Reader.BaseStream.Position + size > stopwatch.ElapsedMilliseconds * waveFormat.AverageBytesPerSecond /1000 )
                {
                    //Quick implementation to keep audio output and transscription roughly aligned
                    Task.Delay(100).Wait();
                }
            }

            return Reader.BaseStream.Read(dataBuffer, 0, (int)size);
        }

        public override void Close()
        {
            Reader.Close();
        }

        public void Reset()
        {
            Reader.BaseStream.Position = 0;
        }

        public async Task TrainSpeakers(ConversationTranscriber transcriber)
        {
            transcriber.Transcribed += Transcriber_Transcribed;
            await transcriber.StartTranscribingAsync();
            await stopTrainingSpeakers.Task;
            await transcriber.StopTranscribingAsync();
            await Task.Delay(2000);
            Reader.BaseStream.Position = 0;

        }

        private async void Transcriber_Transcribed(object sender, ConversationTranscriptionEventArgs e)
        {
            //Check if we have two Identified Guest Users
            switch(e.Result.SpeakerId.ToLower())
            {
                case "guest-1":
                    Speaker1Identified = true;
                    break;
                case "guest-2":
                    Speaker2Identified = true;
                    break;
            }

            if(Speaker1Identified &&  Speaker2Identified)
            {
                ((ConversationTranscriber)sender).Transcribed -= Transcriber_Transcribed;
                await ((ConversationTranscriber)sender).StopTranscribingAsync();
                stopTrainingSpeakers.TrySetResult(0);
                TrainingDone = true;
            }
        }
    }
}
