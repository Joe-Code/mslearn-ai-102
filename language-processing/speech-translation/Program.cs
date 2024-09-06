using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Translation;

namespace speech_translation
{
    class Program
    {
        private static SpeechConfig speechConfig;
        private static SpeechTranslationConfig translationConfig;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, AI Language Processing World with Speech Translation!");

            try
            {
                // Set the Speech Service key
                var speechServiceKey = Environment.GetEnvironmentVariable("SPEECHSERVICE_KEY");
                if (string.IsNullOrEmpty(speechServiceKey))
                {
                    throw new InvalidOperationException("Environment variable SPEECHSERVICE_KEY is not set.");
                }
                Console.WriteLine("Azure Speech Service key was set.");

                // Set the Speech Service region
                var speechServiceRegion = Environment.GetEnvironmentVariable("SPEECHSERVICE_REGION");
                if (string.IsNullOrEmpty(speechServiceRegion))
                {
                    throw new InvalidOperationException("Environment variable SPEECHSERVICE_REGION is not set.");
                }
                Console.WriteLine("Azure Speech Service region was set: " + speechServiceRegion);

                // Set console encoding to unicode
                Console.InputEncoding = Encoding.Unicode;
                Console.OutputEncoding = Encoding.Unicode;

                // Configure translation
                translationConfig = SpeechTranslationConfig.FromSubscription(speechServiceKey, speechServiceRegion);
                translationConfig.SpeechRecognitionLanguage = "en-US";
                translationConfig.AddTargetLanguage("fr");
                translationConfig.AddTargetLanguage("es");
                translationConfig.AddTargetLanguage("hi");
                Console.WriteLine("\nReady to translate from " + translationConfig.SpeechRecognitionLanguage);

                // Configure speech
                speechConfig = SpeechConfig.FromSubscription(speechServiceKey, speechServiceRegion);
                Console.WriteLine("Ready to use speech service:\n");

                // Ask user what type of audio they want to use
                Console.WriteLine("Do you want to use\n 1. Microphone\n 2. Wave File\nEnter 1 or 2:");
                var useFile = Console.ReadLine() ?? "2";
                Console.WriteLine($"You selected: {useFile}\n");

                //Convert useFile to a boolean
                bool useWaveFile = useFile == "2" ? true : false;
                Console.WriteLine($"You selected: {useWaveFile}\n");

                // Get target language
                Console.WriteLine("\nEnter a target language\n fr = French\n es = Spanish\n hi = Hindi\n Enter anything else to stop\n");
                var targetLanguage = Console.ReadLine().ToLower() ?? "stop";

                if (translationConfig.TargetLanguages.Contains(targetLanguage))
                {
                    // Create AudioConfig object for use with either microphone or wave file input
                    AudioConfig audioConfig;

                    if (useWaveFile)
                    {
                        // Get the wave file
                        audioConfig = AudioConfig.FromWavFileInput("station.wav");
                        TranslateAudioFile(translationConfig, audioConfig, targetLanguage).Wait();
                    }
                    else
                    {
                        // Use the microphone
                        audioConfig = AudioConfig.FromDefaultMicrophoneInput();
                        TranslateMicrophoneInput(translationConfig, audioConfig, targetLanguage).Wait();
                    }
                }
                else
                {
                    Console.WriteLine("Invalid target language. Exiting...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static async Task TranslateMicrophoneInput(SpeechTranslationConfig translationConfig, AudioConfig audioConfig, string targetLanguage)
        {
            using TranslationRecognizer translator = new TranslationRecognizer(translationConfig, audioConfig);
            Console.WriteLine("Speak now...");

            TranslationRecognitionResult result = await translator.RecognizeOnceAsync();

            // Analyze the result
            if (result.Reason == ResultReason.TranslatedSpeech)
            {
                Console.WriteLine($"Translating: '{result.Text}'");

                // Display the translation for only the target language
                Console.WriteLine($"{targetLanguage}: {result.Translations[targetLanguage]}");

                SynthesizeTranslation(translationConfig, targetLanguage, result.Text).Wait();
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                Console.WriteLine("No speech could be recognized.");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                Console.WriteLine($"CANCELED: Reason={result.Reason}");
            }
        }

        static async Task TranslateAudioFile(SpeechTranslationConfig translationConfig, AudioConfig audioConfig, string targetLanguage)
        {
            using TranslationRecognizer translator = new TranslationRecognizer(translationConfig, audioConfig);
            Console.WriteLine("Translating audio file...");

            TranslationRecognitionResult result = await translator.RecognizeOnceAsync();
            if (result.Reason == ResultReason.TranslatedSpeech)
            {
                Console.WriteLine($"Translating: '{result.Text}'");
                
                // Display the translation for only the target language
                Console.WriteLine($"{targetLanguage}: {result.Translations[targetLanguage]}");

                SynthesizeTranslation(translationConfig, targetLanguage, result.Text).Wait();
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.WriteLine("No speech could be recognized.");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                Console.WriteLine($"CANCELED: Reason={result.Reason}");
            }
        }

        static async Task SynthesizeTranslation(SpeechTranslationConfig translationConfig, string targetLanguage, string translation)
        {
            // Synthesize translation
            var voices = new Dictionary<string, string>
            {
                ["fr"] = "fr-FR-HenriNeural",
                ["es"] = "es-ES-ElviraNeural",
                ["hi"] = "hi-IN-MadhurNeural"
            };

            translationConfig.SpeechSynthesisVoiceName = voices[targetLanguage];
            using SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer(translationConfig);
            Console.WriteLine("Synthesizing translation...");

            SpeechSynthesisResult speaker = await speechSynthesizer.SpeakTextAsync(translation);
            if (speaker.Reason != ResultReason.SynthesizingAudioCompleted)
            {
                Console.WriteLine(speaker.Reason);
            }
        }
    }
}
