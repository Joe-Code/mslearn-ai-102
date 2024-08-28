using System;
using Azure;
using Azure.AI.Vision.Face;
// using System.IO;
// using System.Linq;
// using System.Drawing;
// using System.Collections.Generic;
// using System.Threading.Tasks;

// Import namespaces



namespace analyzeimage
{
    class analyze_faces
    {
        static async Task FaceAnalyzer(string[] args)
        {
            try
            {
                Console.WriteLine("Hello, AI Vision World!");

                // Set the Azure OpenAI endpoint
                var azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZUREOPENAI_ENDPOINT");
                if (string.IsNullOrEmpty(azureOpenAIEndpoint))
                {
                    throw new InvalidOperationException("Environment variable AZUREOPENAI_ENDPOINT is not set.");
                }
                Console.WriteLine("Azure OpenAI endpoint was set: " + azureOpenAIEndpoint);

                // Set the Azure OpenAI key
                var azureOpenAIKey = Environment.GetEnvironmentVariable("AZUREOPENAI_KEY");
                if (string.IsNullOrEmpty(azureOpenAIKey))
                {
                    throw new InvalidOperationException("Environment variable AZUREOPENAI_KEY is not set.");
                }
                Console.WriteLine("Azure OpenAI key was set.");

                // Set image file
                string imageFile = "people.jpg";
                if (args.Length > 0)
                {
                    imageFile = args[0];
                }


                // Authenticate Azure AI Vision client
                
                string imageFilePath = Path.Combine(Directory.GetCurrentDirectory(), "images", imageFile);


                // Menu for face functions
                Console.WriteLine("1: Detect faces\nAny other key to quit");
                Console.WriteLine("Enter a number:");
                string command = Console.ReadLine() ?? string.Empty;
                switch (command)
                {
                    case "1":
                        await DetectFaces("images/people.jpg");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static async Task DetectFaces(string imageFile)
        {
            Console.WriteLine($"Detecting faces in {imageFile}");

            // Specify facial features to be retrieved


            // Get faces
 
 
        }
    }
}