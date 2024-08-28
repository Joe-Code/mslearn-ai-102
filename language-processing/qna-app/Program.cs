using System;
using Azure;
using Azure.AI.Language.QuestionAnswering;

namespace qna_app
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello, AI Language World! \nLet's work with Question and Answer of the Azure Language Service.");

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

                var projectName = "LearnFAQ";
                var deploymentName = "production";

                // Submit a question and display the answer
                string userQuestion = "";
                while (userQuestion != "exit")
                {
                    // Ask user if they want to use a Project or Not for their question
                    Console.WriteLine("Do you want to use\n 1. Azure Language Service Project\n 2. Provided Text Records\nEnter 1 or 2:");
                    var useProject = Console.ReadLine();
                    int choice = useProject != null ? int.Parse(useProject) : 1;

                    Console.WriteLine("Please enter a question or type 'exit' to quit:");
                    userQuestion = Console.ReadLine() ?? string.Empty;

                    if (userQuestion == string.Empty)
                    {
                        continue;
                    }
                    else if (userQuestion != "exit")
                    {
                        QuestionAnsweringClient client = new QuestionAnsweringClient(new Uri(languageServiceEndpoint), new AzureKeyCredential(languageServiceKey));
                        if (choice == 1)
                        {
                            AskQuestionInProject(client, projectName, deploymentName, userQuestion);
                        }
                        else
                        {
                            AskQuestionFromText(client, userQuestion);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void AskQuestionInProject(QuestionAnsweringClient client, string projectName, string deploymentName, string userQuestion)
        {
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);
            Response<AnswersResult> response = client.GetAnswers(userQuestion, project);

            foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
            {
                Console.WriteLine($"\nQ: {userQuestion}");
                Console.WriteLine($"A: {answer.Answer}");
                Console.WriteLine($"Confidence: {answer.Confidence:P2}");
                Console.WriteLine($"Source: {answer.Source}\n");
            }
        }

        private static void AskQuestionFromText(QuestionAnsweringClient client, string userQuestion)
        {
            IEnumerable<TextDocument> records = new[]
            {
                new TextDocument("doc1", "Power and charging.It takes two to four hours to charge the Surface Pro 4 battery fully from an empty state. " +
                         "It can take longer if you're using your Surface for power-intensive activities like gaming or video streaming while you're charging it"),
                new TextDocument("doc2", "You can use the USB port on your Surface Pro 4 power supply to charge other devices, like a phone, while your Surface charges. " +
                         "The USB port on the power supply is only for charging, not for data transfer. If you want to use a USB device, plug it into the USB port on your Surface."),
            };

            // AnswersFromTextOptions options = new AnswersFromTextOptions("How long does it takes to charge a surface?", records);
            AnswersFromTextOptions options = new AnswersFromTextOptions(userQuestion, records);
            Response<AnswersFromTextResult> response = client.GetAnswersFromText(options);

            foreach (TextAnswer answer in response.Value.Answers)
            {
                if (answer.Confidence > .9)
                {
                    string BestAnswer = response.Value.Answers[0].Answer;

                    Console.WriteLine($"Q: {options.Question}");
                    Console.WriteLine($"A: {BestAnswer}");
                    Console.WriteLine($"Confidence Score: ({response.Value.Answers[0].Confidence:P2})"); //:P2 converts the result to a percentage with 2 decimals of accuracy. 
                    break;
                }
                else
                {
                    Console.WriteLine($"Q: {options.Question}");
                    Console.WriteLine("No answers met the requested confidence score.");
                    break;
                }
            }
        }
    }
}