using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Media;

namespace speaking_clock
{

    class Program
    {
        private static SpeechConfig speechConfig;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, AI Language Processing World with Speech!");

            try
            {

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

                // Set the Azure OpenAI region
                var azureOpenAIRegion = Environment.GetEnvironmentVariable("AZUREOPENAI_REGION");
                if (string.IsNullOrEmpty(azureOpenAIRegion))
                {
                    throw new InvalidOperationException("Environment variable AZUREOPENAI_REGION is not set.");
                }
                Console.WriteLine("Azure OpenAI region was set: " + azureOpenAIRegion);

                // Configure speech service
                speechConfig = SpeechConfig.FromSubscription(azureOpenAIKey, azureOpenAIRegion);
                Console.WriteLine("Ready to use speech service:\n");

                // Configure voice
                speechConfig.SpeechSynthesisVoiceName = "en-US-AriaNeural";

                // Ask user what type of audio they want to use
                Console.WriteLine("Do you want to use\n 1. Microphone\n 2. Wave File\nEnter 1 or 2:");
                var useFile = Console.ReadLine() ?? "2";

                //Convert useFile to a boolean
                bool useWaveFile = useFile == "2";

                // Get spoken input
                string command = "";
                AudioConfig audioConfig;

                if (useWaveFile)
                {
                    // Configure speech recognition for file input
                    string audioFile = "time.wav";
                    SoundPlayer wavPlayer = new SoundPlayer(audioFile);
                    wavPlayer.Play();

                    audioConfig = AudioConfig.FromWavFileInput(audioFile);
                }
                else
                {
                    // Configure speech recognition for microphone input
                    audioConfig = AudioConfig.FromDefaultMicrophoneInput();
                    Console.WriteLine("Speak now...");
                }

                if (audioConfig != null)
                {
                    command = await TranscribeCommand(audioConfig);
                    await TellTime(command);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static async Task<string> TranscribeCommand(AudioConfig audioConfig)
        {
            string command = "";
            using SpeechRecognizer speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            // Process speech input
            SpeechRecognitionResult speech = await speechRecognizer.RecognizeOnceAsync();
            if (speech.Reason == ResultReason.RecognizedSpeech)
            {
                command = speech.Text;
                Console.WriteLine(command);
            }
            else
            {
                Console.WriteLine(speech.Reason);
                if (speech.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(speech);
                    Console.WriteLine(cancellation.Reason);
                    Console.WriteLine(cancellation.ErrorDetails);
                }
            }

            // Return the command
            return command;
        }

        static async Task TellTime(string command)
        {
            var now = DateTime.Now;
            // Use the response text if the command is "What time is it?"; Return the actual time and not just what the user said
            string responseText = command.ToLower() == "what time is it?" ? "The time is " + now.Hour.ToString() + ":" + now.Minute.ToString("D2") : command;

            // Configure speech synthesis
            speechConfig.SpeechSynthesisVoiceName = "en-GB-LibbyNeural";
            using SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(speechConfig);

            // Synthesize spoken output
            // SpeechSynthesisResult speak = await speechSynthesizer.SpeakTextAsync(responseText);
            // if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
            // {
            //     Console.WriteLine(speak.Reason);
            // }

            string responseSsml = $@"
                <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
                    <voice name='en-GB-RyanNeural'>
                        {responseText}
                        <break strength='weak'/>
                        Time to end this lab!
                    </voice>
                </speak>";
            SpeechSynthesisResult speak = await speechSynthesizer.SpeakSsmlAsync(responseSsml);
            if (speak.Reason != ResultReason.SynthesizingAudioCompleted)
            {
                Console.WriteLine(speak.Reason);
            }

            if (responseText == "what time is it?")
            {
                // Print the response
                Console.WriteLine(responseText);
            }
        }
    }
}