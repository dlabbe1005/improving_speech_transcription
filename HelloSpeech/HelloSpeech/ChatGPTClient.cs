using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;

using Azure.AI.OpenAI;
using static HelloSpeech.Topic;

namespace HelloSpeech
{
    internal class ChatGPTClient
    {
        private string OpenAIModel = "mychat2";
        const string ChatGPTSystemMessage = @"Your are an AI Assistant evaluating a conversation between two speakers marked left and right. Only answer if you know the reply. Reply with the name only in two to three words.";
        const string ChatGPTPromptIdentifySpeaker =
@"Who is the speaker {0}: in the following conversation. Reply with the name only in two to three words    
---
CONVERSATION
{1}
---
Reply with the name of speaker {0}: only. Use two or three words or if no name is available use a short description. No explanation. Direct Answer.";


        public const string ChatGPTPromptTopic =
@"What is the topic of the conversation, answer with one of the Following Options:
    'Sign-up for an insurance'
    'Get a prescription'
    'Unknown'
---
CONVERSATION
{0}
---
Only reply with one of the three options.  Do not add any additional words to the options. Only use the words in the three optiosns. Do not add 'The Topic of the Conversation is'. No explanation. Direct Answer.";

        public const string ChatGPTPromptTopicNumbers =
@"What is the topic of the conversation, answer with one of the Following Options:
    '0: Unknown'
    '1: Sign-up for health insurance insurance'
    '2: Get a prescription'
---
CONVERSATION
{0}
---
Only reply with the number of the option. Only reply with 0, 1 or 2 . Only reply with a single number. No explanation. Direct Answer.";


        private const string ChatGPTPromptTalkingPoint =
@"Look at the following conversation. Answer the question: '{0}'.
---
CONVERSATION
{1}
---
Only answer with 'true' or 'false'. If you do not know, answer with 'false'. No explanation. Direct Answer.";


        readonly OpenAIClient client;

        public ChatGPTClient(string endpoint, string key, string depName)
        {
            this.client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            this.OpenAIModel = depName;
        }

        public async Task<string> GetSpeakerName(string channelName, string conversation)
        {
            Response<ChatCompletions> responseWithoutStream = await client.GetChatCompletionsAsync(
                new ChatCompletionsOptions()
                {
                    Messages =
                    {
            new ChatMessage(ChatRole.System, ChatGPTSystemMessage),
            new ChatMessage(ChatRole.User, String.Format(ChatGPTPromptIdentifySpeaker, channelName, conversation))
                    },
                    Temperature = (float)0.001,
                    MaxTokens = 100,
                    DeploymentName = OpenAIModel
                }) ;

            ChatCompletions completions = responseWithoutStream.Value;

            return completions.Choices[0].Message.Content;
        }

        public async Task<string> GetPrompt(string conversation, string prompt, string systemMessage)
        {
            Response<ChatCompletions> responseWithoutStream = await client.GetChatCompletionsAsync(
                
                new ChatCompletionsOptions()
                {
                    Messages =
                    {
                        new ChatMessage(ChatRole.System, systemMessage),
                        new ChatMessage(ChatRole.User, String.Format(prompt, conversation))
                    },
                    Temperature = (float)0.2,
                    MaxTokens = 1,
                    FrequencyPenalty = 0,
                    PresencePenalty = 0,
                    DeploymentName = OpenAIModel
                });

            ChatCompletions completions = responseWithoutStream.Value;
            return completions.Choices[0].Message.Content;
        }

        private async Task CheckTalkingPoint(string conversation, TalkingPoint tp, string systemMessage)
        {
            Response<ChatCompletions> responseWithoutStream = await client.GetChatCompletionsAsync(
            new ChatCompletionsOptions()
            {
                Messages =
                    {
                        new ChatMessage(ChatRole.System, systemMessage),
                        new ChatMessage(ChatRole.User, String.Format(ChatGPTPromptTalkingPoint,tp.ChatGPTPrompt, conversation))
                    },
                Temperature = (float)0.0,
                NucleusSamplingFactor = (float)0.95,
                MaxTokens = 1,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
                DeploymentName= OpenAIModel
                });

            ChatCompletions completions = responseWithoutStream.Value;
            if (Boolean.TryParse(completions.Choices[0].Message.Content.Trim('.'), out bool resultValue))
                tp.State = resultValue;
            else
                Console.WriteLine("break");
        }

        public async Task CheckTopicTalkingPoints(string conversation, Topic topic)
        {
            var tasks = new List<Task>();
            foreach (var talkingPoint in topic.TalkingPoints)
            {
               tasks.Add(CheckTalkingPoint(conversation, talkingPoint, "Only answer with 'true' or 'false'!"));
            }

            await Task.WhenAll(tasks.ToArray());

        }



    }
}
