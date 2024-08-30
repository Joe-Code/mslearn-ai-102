using Azure;
using Azure.AI.TextAnalytics;

namespace classify_text
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {

                Console.WriteLine("Hello, AI Language World! \nLet's work with Conversational Language of the Azure Language Service.");

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
                Console.WriteLine("Azure Language Service key was set.\n\n");

                var projectName = "ClassificationProject";
                var deploymentName = "production";

                // Create a new TextAnlayticsClient
                var client = new TextAnalyticsClient(new Uri(languageServiceEndpoint), new AzureKeyCredential(languageServiceKey));

                // Read each text file in the articles folder
                List<string> batchedDocuments = new List<string>();

                var folderPath = Path.GetFullPath("./articles");
                DirectoryInfo folder = new DirectoryInfo(folderPath);
                FileInfo[] files = folder.GetFiles("*.txt");
                foreach (var file in files)
                {
                    // Read the file contents
                    StreamReader sr = file.OpenText();
                    var text = sr.ReadToEnd();
                    sr.Close();
                    batchedDocuments.Add(text);
                }

                // Get Classifications
                ClassifyDocumentOperation operation = await client.SingleLabelClassifyAsync(WaitUntil.Completed, batchedDocuments, projectName, deploymentName);

                int fileNo = 0;
                await foreach (ClassifyDocumentResultCollection documentsInPage in operation.Value)
                {
                    foreach (ClassifyDocumentResult documentResult in documentsInPage)
                    {
                        Console.WriteLine(files[fileNo].Name);
                        if (documentResult.HasError)
                        {
                            Console.WriteLine($"  Error!");
                            Console.WriteLine($"  Document error code: {documentResult.Error.ErrorCode}");
                            Console.WriteLine($"  Message: {documentResult.Error.Message}");
                            continue;
                        }

                        Console.WriteLine($"  Predicted the following class:");
                        Console.WriteLine();

                        foreach (ClassificationCategory classification in documentResult.ClassificationCategories)
                        {
                            Console.WriteLine($"  Category: {classification.Category}");
                            Console.WriteLine($"  Confidence score: {classification.ConfidenceScore}");
                            Console.WriteLine();
                        }
                        fileNo++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

        }
    }
}