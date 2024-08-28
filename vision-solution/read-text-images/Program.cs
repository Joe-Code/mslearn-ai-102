#pragma warning disable CA1416 // Validate platform compatibility
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using System;
using System.Drawing;

namespace readtextimages
{
    class Program
    {
        static void Main(string[] args)
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

                var client = new ImageAnalysisClient(new Uri(azureOpenAIEndpoint), new AzureKeyCredential(azureOpenAIKey));

                // Menu for text reading functions
                Console.WriteLine("\n1: Use Read API for image (Lincoln.jpg)\n2: Read handwriting (Note.jpg)\nAny other key to quit\n");
                Console.WriteLine("Enter a number:");

                string command = Console.ReadLine() ?? string.Empty;

                switch (command)
                {
                    case "1":
                        GetTextRead("Lincoln.jpg", client);
                        break;
                    case "2":
                        GetTextRead("Note.jpg", client);
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

        static void GetTextRead(string imageFile, ImageAnalysisClient client)
        {
            Console.WriteLine($"\nReading text from {imageFile} \n");
            string imageFilePath = Path.Combine(Directory.GetCurrentDirectory(), "images", imageFile);

            // Use a file stream to pass the image data to the analyze call
            using FileStream stream = new FileStream(imageFilePath, FileMode.Open);

            // Use Analyze image function to read text in image
            ImageAnalysisResult result = client.Analyze(BinaryData.FromStream(stream), VisualFeatures.Read);

            stream.Close();

            // Display Analysis results
            if (result.Read != null)
            {
                Console.WriteLine("Text Read:");

                // Prepare image for drawing
                System.Drawing.Image image = System.Drawing.Image.FromFile(imageFilePath);
                Graphics graphics = Graphics.FromImage(image);
                Pen pen = new Pen(Color.Cyan, 2);

                foreach (var line in result.Read.Blocks.SelectMany(block => block.Lines))
                {
                    // Return the text detected in the image
                    Console.WriteLine($"   {line.Text}");

                    // Draw bounding box around line
                    var drawLinePolygon = true;

                    // Return the position bounding box around each line
                    Console.WriteLine($"   Bounding Polygon: [{string.Join(" ", line.BoundingPolygon)}]");

                    // Return each word detected in the image and the position bounding box around each word with the confidence level of each word
                    foreach (DetectedTextWord word in line.Words)
                    {
                        Console.WriteLine($"     Word: '{word.Text}', Confidence {word.Confidence:F4}, Bounding Polygon: [{string.Join(" ", word.BoundingPolygon)}]");

                        // Draw word bounding polygon
                        drawLinePolygon = false;
                        var r = word.BoundingPolygon;

                        Point[] polygonPoints = {
                            new Point(r[0].X, r[0].Y),
                            new Point(r[1].X, r[1].Y),
                            new Point(r[2].X, r[2].Y),
                            new Point(r[3].X, r[3].Y)
                        };

                        graphics.DrawPolygon(pen, polygonPoints);
                    }

                    // Draw line bounding polygon
                    if (drawLinePolygon)
                    {
                        var r = line.BoundingPolygon;

                        Point[] polygonPoints = {
                            new Point(r[0].X, r[0].Y),
                            new Point(r[1].X, r[1].Y),
                            new Point(r[2].X, r[2].Y),
                            new Point(r[3].X, r[3].Y)
                        };

                        graphics.DrawPolygon(pen, polygonPoints);
                    }
                }

                // Save image
                String output_file = Path.Combine(Directory.GetCurrentDirectory(), "images", "text.jpg");
                image.Save(output_file);
                Console.WriteLine("\nResults saved in " + output_file + "\n");

                // Clean up
                graphics.Dispose();
                image.Dispose();
            }

        }
    }
}