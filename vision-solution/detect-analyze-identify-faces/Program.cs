#pragma warning disable CA1416 // Validate platform compatibility
using Azure.AI.Vision.ImageAnalysis;
using Azure;
using System.Drawing;
using System.Net.Http.Headers;

namespace detectanalyzeidentifyfaces
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

                // Set image file
                string imageFile = "people.jpg";
                if (args.Length > 0)
                {
                    imageFile = args[0];
                }

                // Authenticate Azure AI Vision client
                var client = new ImageAnalysisClient(new Uri(azureOpenAIEndpoint), new AzureKeyCredential(azureOpenAIKey));
                string imageFilePath = Path.Combine(Directory.GetCurrentDirectory(), "images", imageFile);

                // Analyze image
                AnalyzeImage(imageFilePath, client);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void AnalyzeImage(string imageFilePath, ImageAnalysisClient client)
        {
            using FileStream fs = new FileStream(imageFilePath, FileMode.Open);

            // Analyze image
            ImageAnalysisResult result = client.Analyze(BinaryData.FromStream(fs), VisualFeatures.Caption | VisualFeatures.DenseCaptions | VisualFeatures.Objects | VisualFeatures.Tags | VisualFeatures.People);

            // Display analysis results
            // Get image captions
            if (result.Caption.Text != null)
            {
                Console.WriteLine(" Caption:");
                Console.WriteLine($"   \"{result.Caption.Text}\", Confidence {result.Caption.Confidence:0.00}\n");
            }

            // Get image dense captions
            Console.WriteLine(" Dense Captions:");
            foreach (DenseCaption denseCaption in result.DenseCaptions.Values)
            {
                Console.WriteLine($"   Caption: '{denseCaption.Text}', Confidence: {denseCaption.Confidence:0.00}");
            }

            // Get image tags
            if (result.Tags.Values.Count > 0)
            {
                Console.WriteLine($"\n Tags:");
                foreach (DetectedTag tag in result.Tags.Values)
                {
                    Console.WriteLine($"   '{tag.Name}', Confidence: {tag.Confidence:F2}");
                }
            }

            // Get people in the image
            GetPeopleInImage(result, imageFilePath, fs);
        }
        
        static void GetPeopleInImage(ImageAnalysisResult result, string imageFilePath, Stream stream)
        {
            if (result.People.Values.Count > 0)
            {
                Console.WriteLine(" People:");

                // Prepare image for drawing
                stream.Close();

                Image image = Image.FromFile(imageFilePath);
                Graphics graphics = Graphics.FromImage(image);
                Pen pen = new Pen(Color.Cyan, 3);
                Font font = new Font("Arial", 16);
                SolidBrush brush = new SolidBrush(Color.WhiteSmoke);

                foreach (DetectedPerson detectedPerson in result.People.Values)
                {
                    // Draw object bounding box
                    var r = detectedPerson.BoundingBox;
                    Rectangle rect = new Rectangle(r.X, r.Y, r.Width, r.Height);
                    graphics.DrawRectangle(pen, rect);

                    // Return the confidence of the person detected
                    Console.WriteLine($"   Bounding box {detectedPerson.BoundingBox.ToString()}, Confidence: {detectedPerson.Confidence:F2}");
                }

                // Save annotated image
                String output_file = "images/output/people.jpg";
                image.Save(output_file);
                Console.WriteLine("  Results saved in " + output_file + "\n");
            }
        }
    }
}