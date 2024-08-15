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

                // Set the Computer Vision Endpoint
                var computerVisionEndpoint = Environment.GetEnvironmentVariable("COMPUTERVISION_ENDPOINT");
                if (string.IsNullOrEmpty(computerVisionEndpoint))
                {
                    throw new InvalidOperationException("Environment variable COMPUTERVISION_ENDPOINT is not set.");
                }
                Console.WriteLine("Azure Computer Vision Endpoint was set: " + computerVisionEndpoint + "\n");

                // Set the Computer Vision Endpoint
                var computerVisionEndpointKey = Environment.GetEnvironmentVariable("COMPUTERVISION_ENDPOINTKEY");
                if (string.IsNullOrEmpty(computerVisionEndpointKey))
                {
                    throw new InvalidOperationException("Environment variable COMPUTERVISION_ENDPOINTKEY is not set.\n\n");
                }
                Console.WriteLine("Azure Computer Vision Endpoint Key was set.");

                // Set image file
                string imageFile = "person.jpg";
                if (args.Length > 0)
                {
                    imageFile = args[0];
                }

                // Authenticate Azure AI Vision client
                var client = new ImageAnalysisClient(new Uri(computerVisionEndpoint), new AzureKeyCredential(computerVisionEndpointKey));
                string imageFilePath = Path.Combine(Directory.GetCurrentDirectory(), "images", imageFile);

                // Analyze image
                AnalyzeImage(imageFilePath, client);

                // Remove the background or generate a foreground matte from the image
                await BackgroundForeground(imageFilePath, computerVisionEndpoint, computerVisionEndpointKey);

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

        static async Task BackgroundForeground(string imageFilePath, string endpoint, string key)
        {
            // ***IMPORTANT***
            // With the Image Analysis 4.0 API, Background removal is only available through direct REST API calls. It is not available through the SDKs.
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
                        File.WriteAllBytes("background.png", response.Content.ReadAsByteArrayAsync().Result);
                        Console.WriteLine("  Results saved in background.png\n");
                    }
                    else
                    {
                        Console.WriteLine($"API error: {response.ReasonPhrase} - Check your body url, key, and endpoint.");
                    }
                }
            }

            // Define the API version and mode
            // string apiVersion = "2023-02-01-preview";
            // string mode = "backgroundRemoval"; // Can be "foregroundMatting" or "backgroundRemoval"
            // string url = $"computervision/imageanalysis:segment?api-version={apiVersion}&mode={mode}";

            // Make the REST call
            // using (var client = new HttpClient())
            // {
            //     var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            //     client.BaseAddress = new Uri(endpoint);
            //     client.DefaultRequestHeaders.Accept.Add(contentType);
            //     client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

            //     var data = new
            //     {
            //         url = $"https://github.com/MicrosoftLearning/mslearn-ai-vision/blob/main/Labfiles/01-analyze-images/Python/image-analysis/{imageFile}?raw=true"
            //     };

            //     var jsonData = JsonSerializer.Serialize(data);
            //     var contentData = new StringContent(jsonData, Encoding.UTF8, contentType);
            //     var response = await client.PostAsync(url, contentData);

            //     if (response.IsSuccessStatusCode)
            //     {
            //         File.WriteAllBytes("background.png", response.Content.ReadAsByteArrayAsync().Result);
            //         Console.WriteLine("  Results saved in background.png\n");
            //     }
            //     else
            //     {
            //         Console.WriteLine($"API error: {response.ReasonPhrase} - Check your body url, key, and endpoint.");
            //     }
            // }
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
                String output_file = "objects.jpg";
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
                String output_file = "people.jpg";
                image.Save(output_file);
                Console.WriteLine("  Results saved in " + output_file + "\n");
            }
        }
    }
}


