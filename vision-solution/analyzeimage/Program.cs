#pragma warning disable CA1416 // Validate platform compatibility
using Azure.AI.Vision.ImageAnalysis;
using Azure;
using System.Drawing;
using System.Net.Http.Headers;

namespace analyzeimage
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello, AI Vision World!");

                // Set the Azure Open AI Endpoint
                var azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZUREOPENAI_ENDPOINT");
                if (string.IsNullOrEmpty(azureOpenAIEndpoint))
                {
                    throw new InvalidOperationException("Environment variable AZUREOPENAI_ENDPOINT is not set.");
                }
                Console.WriteLine("Azure Open AI Endpoint was set: " + azureOpenAIEndpoint + "\n");

                // Set the Azure Open AI Key
                var azureOpenAIKey = Environment.GetEnvironmentVariable("AZUREOPENAI_KEY");
                if (string.IsNullOrEmpty(azureOpenAIKey))
                {
                    throw new InvalidOperationException("Environment variable AZUREOPENAI_KEY is not set.\n\n");
                }
                Console.WriteLine("Azure Open AI Key was set.");

                // Ask user what pic they want to use
                Console.WriteLine("Do you want to use\n 1. Bio Pic\n 2. Building\n 3. Person(Satya)\n 4. Street\nEnter 1,2,3 or 4:");
                var useFile = Console.ReadLine() ?? "1";
                
                // Set image file
                string imageFile;

                switch (useFile)
                {
                    case "1":
                        Console.WriteLine("You selected Bio Pic");
                        imageFile = "BioPic.jpg";
                        break;
                    case "2":
                        Console.WriteLine("You selected Building");
                        imageFile = "Building.jpg";
                        break;
                    case "3":
                        Console.WriteLine("You selected Person (Satya)");
                        imageFile = "Person.jpg";
                        break;
                    case "4":
                        Console.WriteLine("You selected Street");
                        imageFile = "Street.jpg";
                        break;
                    default:
                        Console.WriteLine("You selected Bio Pic");
                        imageFile = "BioPic.jpg";
                        break;
                }

                // Authenticate Azure AI Vision client
                var client = new ImageAnalysisClient(new Uri(azureOpenAIEndpoint), new AzureKeyCredential(azureOpenAIKey));
                string imageFilePath = Path.Combine(Directory.GetCurrentDirectory(), "images", imageFile);

                // Analyze image
                AnalyzeImage(imageFilePath, client);

                // Remove the background or generate a foreground matte from the image
                await BackgroundForeground(imageFile, imageFilePath, azureOpenAIEndpoint, azureOpenAIKey);

                Console.WriteLine("Good Bye! I hope you enjoyed your experience with AI Vision World!");
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

            // Get objects in the image
            GetObjectsInImage(result, imageFilePath, fs);

            // Get people in the image
            GetPeopleInImage(result, imageFilePath, fs);
        }

        static async Task BackgroundForeground(string imageFile, string imageFilePath, string endpoint, string key)
        {
            // ***IMPORTANT***
            // With the Image Analysis 4.0 API, Background removal is only available through direct REST API calls. It is not available through the SDKs.
            // This is why the azure ai services sdk is not used in this method and the endpoint and key are passed as parameters.
            // The following code demonstrates how to make a direct REST API call to remove the background from an image or generate a foreground matte.
            // ***IMPORTANT***

            // Remove the background from the image or generate a foreground matte
            Console.WriteLine($" Background removal:");

            string mode = "backgroundRemoval"; // Can be "foregroundMatting" or "backgroundRemoval"
            byte[] imageBytes = File.ReadAllBytes(imageFilePath);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
                string requestUri = $"{endpoint}/computervision/imageanalysis:segment?api-version=2023-02-01-preview&mode={mode}";

                using (var content = new ByteArrayContent(imageBytes))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    HttpResponseMessage response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();

                    if (response.IsSuccessStatusCode)
                    {
                        File.WriteAllBytes($"images/output/output-{imageFile}", response.Content.ReadAsByteArrayAsync().Result);
                        Console.WriteLine("  Results saved in images/output/background.png\n");
                    }
                    else
                    {
                        Console.WriteLine($"API error: {response.ReasonPhrase} - Check your body url, key, and endpoint.");
                    }
                }
            }
        }

        static void GetObjectsInImage(ImageAnalysisResult result, string imageFilePath, Stream stream)
        {
            if (result.Objects.Values.Count > 0)
            {
                Console.WriteLine(" Objects:");

                // Prepare image for drawing
                stream.Close();

                Image image = Image.FromFile(imageFilePath);
                Graphics graphics = Graphics.FromImage(image);
                Pen pen = new Pen(Color.Cyan, 3);
                Font font = new Font("Arial", 16);
                SolidBrush brush = new SolidBrush(Color.WhiteSmoke);

                foreach (DetectedObject detectedObject in result.Objects.Values)
                {
                    Console.WriteLine($"   \"{detectedObject.Tags[0].Name}\"");

                    // Draw object bounding box
                    var r = detectedObject.BoundingBox;
                    Rectangle rect = new Rectangle(r.X, r.Y, r.Width, r.Height);
                    graphics.DrawRectangle(pen, rect);
                    graphics.DrawString(detectedObject.Tags[0].Name, font, brush, (float)r.X, (float)r.Y);
                }

                // Save annotated image
                String output_file = "images/output/objects.jpg";
                image.Save(output_file);
                Console.WriteLine("  Results saved in " + output_file + "\n");
            }
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


