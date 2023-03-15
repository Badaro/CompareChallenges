using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace CompareChallenges
{
    internal class Program
    {
        static string _rootUrl = "https://badaro.github.io/MTGODecklistCache/Tournaments/mtgo.com";
        static string _outputFile = "event_data.csv";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Invalid number of parameters, you need to specify at least one event");
                return;
            }

            try
            {
                Console.WriteLine($"Comparing cards between:");
                args.ToList().ForEach(a => Console.WriteLine($"* {a}"));

                List<Dictionary<string, int>> combinedEventData = new List<Dictionary<string, int>>();
                args.ToList().ForEach(a => combinedEventData.Add(GetCardsFromEvent(a)));

                StringBuilder output = new StringBuilder();
                output.Append("Card,");
                output.Append(String.Join(",", args));
                output.Append(Environment.NewLine);

                foreach (var card in combinedEventData.SelectMany(i => i.Keys).Distinct().ToList())
                {
                    output.Append($"\"{card}\"");
                    foreach (var eventData in combinedEventData)
                    {
                        if (eventData.ContainsKey(card)) output.Append($",\"{eventData[card]}\"");
                        else output.Append(",\"0\"");
                    }
                    output.Append(Environment.NewLine);
                }

                File.WriteAllText(_outputFile, output.ToString());
                Console.WriteLine($"Result saved to {_outputFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during processing: {ex.Message}");
            }
        }

        private static Dictionary<string, int> GetCardsFromEvent(string eventName)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            string eventDate = String.Join("/", eventName.Split("-").TakeLast(3)).Substring(0, 10);
            string eventUrl = $"{_rootUrl}/{eventDate}/{eventName}.json";
            string eventData = new HttpClient().GetStringAsync(eventUrl).Result;

            dynamic json = JsonConvert.DeserializeObject<dynamic>(eventData);
            foreach (var deck in json.Decks)
            {
                foreach (var card in deck.Mainboard)
                {
                    string name = card.CardName;
                    int count = card.Count;

                    if (!result.ContainsKey(name)) result.Add(name, count);
                    else result[name] += count;
                }
                foreach (var card in deck.Sideboard)
                {
                    string name = card.CardName;
                    int count = card.Count;

                    if (!result.ContainsKey(name)) result.Add(name, count);
                    else result[name] += count;
                }
            }

            return result;
        }
    }
}
