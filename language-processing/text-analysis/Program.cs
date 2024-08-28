using System;
using Azure;
using Azure.AI.TextAnalytics;

namespace readtextimages
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello, AI Language World!");

                // Set the Azure Language Service endpoint
                var languageServiceEndpoint = Environment.GetEnvironmentVariable("LANGUAGESERVICE_ENDPOINT");
                if (string.IsNullOrEmpty(languageServiceEndpoint))
                {
                    throw new InvalidOperationException("Environment variable LANGUAGESERVICE_ENDPOINT is not set.");
                }
                Console.WriteLine("Azure Language Service endpoint was set: " + languageServiceEndpoint);

                // Set the Azure Language Service key
                var languageServiceKey = Environment.GetEnvironmentVariable("LANGUAGESERVICE_KEY");
                if (string.IsNullOrEmpty(languageServiceKey))
                {
                    throw new InvalidOperationException("Environment variable LANGUAGESERVICE_KEY is not set.");
                }
                Console.WriteLine("Azure Language Service key was set.");

                TextAnalyticsClient client = new TextAnalyticsClient(new Uri(languageServiceEndpoint), new AzureKeyCredential(languageServiceKey));

                // Analyze each text file in the reviews folder
                var folderPath = Path.GetFullPath("./reviews");
                Console.WriteLine("Reading text files from: " + folderPath);

                DirectoryInfo folder = new DirectoryInfo(folderPath);
                foreach (var file in folder.GetFiles("*.txt"))
                {
                    // Read the file contents
                    Console.WriteLine("\n-------------\n" + file.Name);
                    StreamReader sr = file.OpenText();
                    var text = sr.ReadToEnd();
                    sr.Close();
                    Console.WriteLine("\n" + text + "\n");

                    // Get language
                    DetectedLanguage detectedLanguage = client.DetectLanguage(text);
                    Console.WriteLine($"\nLanguage: {detectedLanguage.Name}");

                    // Get sentiment
                    DocumentSentiment documentSentiment = client.AnalyzeSentiment(text);
                    Console.WriteLine($"\nSentiment: {documentSentiment.Sentiment}");

                    // Get key phrases
                    KeyPhraseCollection keyPhrases = client.ExtractKeyPhrases(text);
                    if (keyPhrases.Count > 0)
                    {
                        Console.WriteLine("\nKey Phrases:");
                        foreach (string keyPhrase in keyPhrases)
                        {
                            Console.WriteLine($"\t{keyPhrase}");
                        }
                    }

                    // Get entities
                    CategorizedEntityCollection entities = client.RecognizeEntities(text);
                    if (entities.Count > 0)
                    {
                        Console.WriteLine("\nEntities:");
                        foreach (CategorizedEntity entity in entities)
                        {
                            Console.WriteLine($"\t{entity.Text} ({entity.Category})");
                        }
                    }

                    // Get linked entities
                    LinkedEntityCollection linkedEntities = client.RecognizeLinkedEntities(text);
                    if (linkedEntities.Count > 0)
                    {
                        Console.WriteLine("\nLinked Entities:");
                        foreach (LinkedEntity linkedEntity in linkedEntities)
                        {
                            Console.WriteLine($"\t{linkedEntity.Name} ({linkedEntity.DataSource})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}