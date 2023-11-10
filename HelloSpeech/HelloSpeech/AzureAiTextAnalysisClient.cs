using Azure;
using Azure.AI.TextAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloSpeech
{
    internal class AzureAiTextAnalysisClient
    {

        readonly TextAnalyticsClient client;


        public AzureAiTextAnalysisClient(string endpoint, string key)
        {

            client = new TextAnalyticsClient(new Uri(endpoint),
                new AzureKeyCredential(key));
        }

        public async Task<DocumentSentiment> GetSentimentAsync(string input)
        {
            Response <DocumentSentiment> sentiment = await client.AnalyzeSentimentAsync(input);
            return sentiment.Value;
        }

        public async Task<DocumentSentiment> GetSentimentAsync(List<string> inputConversation)
        {
            string combinedConversation = String.Join(Environment.NewLine, inputConversation);
            Response<DocumentSentiment> sentiment = await client.AnalyzeSentimentAsync(combinedConversation);
            return sentiment.Value;
        }



    }
}
