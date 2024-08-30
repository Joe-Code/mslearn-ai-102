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

                // Read each text file in the ads folder
                List<TextDocumentInput> batchedDocuments = new();
                var folderPath = Path.GetFullPath("./ads");
                DirectoryInfo folder = new(folderPath);
                FileInfo[] files = folder.GetFiles("*.txt");
                foreach (var file in files)
                {
                    // Read the file contents
                    StreamReader sr = file.OpenText();
                    var text = sr.ReadToEnd();
                    sr.Close();
                    TextDocumentInput doc = new(file.Name, text)
                    {
                        Language = "en",
                    };
                    batchedDocuments.Add(doc);
                }

                // Extract entities
                RecognizeCustomEntitiesOperation operation = await client.RecognizeCustomEntitiesAsync(WaitUntil.Completed, batchedDocuments, projectName, deploymentName);

                await foreach (RecognizeCustomEntitiesResultCollection documentsInPage in operation.Value)
                {
                    foreach (RecognizeEntitiesResult documentResult in documentsInPage)
                    {
                        Console.WriteLine($"Result for \"{documentResult.Id}\":");

                        if (documentResult.HasError)
                        {
                            Console.WriteLine($"  Error!");
                            Console.WriteLine($"  Document error code: {documentResult.Error.ErrorCode}");
                            Console.WriteLine($"  Message: {documentResult.Error.Message}");
                            Console.WriteLine();
                            continue;
                        }

                        Console.WriteLine($"  Recognized {documentResult.Entities.Count} entities:");

                        foreach (CategorizedEntity entity in documentResult.Entities)
                        {
                            Console.WriteLine($"  Entity: {entity.Text}");
                            Console.WriteLine($"  Category: {entity.Category}");
                            Console.WriteLine($"  Offset: {entity.Offset}");
                            Console.WriteLine($"  Length: {entity.Length}");
                            Console.WriteLine($"  ConfidenceScore: {entity.ConfidenceScore}");
                            Console.WriteLine($"  SubCategory: {entity.SubCategory}");
                            Console.WriteLine();
                        }

                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred:");
                Console.WriteLine(ex.Message);
            }
        }
    }
}