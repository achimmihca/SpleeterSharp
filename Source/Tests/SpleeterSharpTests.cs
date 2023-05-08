using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SpleeterSharp
{
    public class SpleeterSharpTests
    {
        private const string SpleeterCommand = "..\\..\\SpleeterMsvcExe-v1.0\\Spleeter.exe";

        [Fact]
        public async void TestSpleeterVoiceSeparation()
        {
            SpleeterSharpConfig.Create()
                .SetSpleeterCommand(SpleeterCommand)
                .SetIsWindows(true)
                .SetLogAction(text => Debug.WriteLine(text));

            ChangeToTestDirectory();

            // Remove old output files
            if (Directory.Exists("OutputFiles/audio_example"))
            {
                Directory.Delete("OutputFiles/audio_example", true);
            }

            SpleeterParameters spleeterParameters = new SpleeterParameters();
            spleeterParameters.InputFile = "InputFiles/audio_example.mp3";
            spleeterParameters.OutputFolder = "OutputFiles/audio_example.ogg";
            spleeterParameters.Overwrite = true;
            SpleeterResult spleeterResult = await SpleeterUtils.SplitAsync(spleeterParameters, CancellationToken.None);

            Debug.WriteLine($"Spleeter output: {spleeterResult.Output}");
            
            Assert.NotEmpty(spleeterResult.Output);
            Assert.Equal(0, spleeterResult.ExitCode);

            string expectedVocalFilePath = "OutputFiles/audio_example.vocals.ogg";
            string expectedInstrumentalFilePath = "OutputFiles/audio_example.accompaniment.ogg";
            Assert.True(File.Exists(expectedVocalFilePath));
            Assert.True(File.Exists(expectedInstrumentalFilePath));

            int matchingExpectedFilePathCount = spleeterResult.WrittenFiles
                .Select(path => path.Replace("\\", "/"))
                .Count(path => path.Contains(expectedVocalFilePath) || path.Contains(expectedInstrumentalFilePath));
            Assert.True(spleeterResult.WrittenFiles.Count == 2);
            Assert.Equal(2, matchingExpectedFilePathCount);

            Assert.Empty(spleeterResult.Errors);
        }

        [Fact]
        public async void TestSpleeterVoiceSeparationError()
        {
            SpleeterSharpConfig.Create()
                .SetSpleeterCommand(SpleeterCommand)
                .SetIsWindows(true)
                .SetLogAction(text => Debug.WriteLine(text));

            ChangeToTestDirectory();

            SpleeterParameters spleeterParameters = new SpleeterParameters();
            spleeterParameters.InputFile = "InputFiles/audio_example.mp3";
            spleeterParameters.OutputFolder = "OutputFiles/audio_example.ogg";
            spleeterParameters.Overwrite = true;
            spleeterParameters.OutputFileCodec = "FORCE_ERROR";
            SpleeterResult spleeterResult = await SpleeterUtils.SplitAsync(spleeterParameters, CancellationToken.None);

            Assert.NotEmpty(spleeterResult.Output);
            Assert.True(spleeterResult.ExitCode != 0);
            Assert.NotEmpty(spleeterResult.Errors);
            Assert.NotEmpty(spleeterResult.Errors[0]);
        }

        [Fact]
        public async void TestSpleeterVoiceSeparationCanceled()
        {
            SpleeterSharpConfig.Create()
                .SetSpleeterCommand(SpleeterCommand)
                .SetIsWindows(true)
                .SetLogAction(text => Debug.WriteLine(text));

            ChangeToTestDirectory();

            SpleeterParameters spleeterParameters = new SpleeterParameters();
            spleeterParameters.InputFile = "InputFiles/audio_example.mp3";
            spleeterParameters.OutputFolder = "OutputFiles/audio_example.ogg";
            spleeterParameters.Overwrite = true;

            SpleeterResult spleeterResult;
            try
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                Task<SpleeterResult> spleeterResultTask =
                    SpleeterUtils.SplitAsync(spleeterParameters, cancellationTokenSource.Token);

                // Simulate cancellation
                cancellationTokenSource.CancelAfter(500);

                spleeterResult = await spleeterResultTask;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Received exception as expected: {ex.GetType()}");
                Debug.WriteLine(ex.Message + "\n" +ex.StackTrace);
                return;
            }

            Assert.Fail($"Received Spleeter result although task was cancelled: {spleeterResult}");
        }

        private void ChangeToTestDirectory()
        {
            Debug.WriteLine("moving to test directory from: " + Directory.GetCurrentDirectory());
            Directory.SetCurrentDirectory(GetTestDirectory());
            Debug.WriteLine("current directory: " + Directory.GetCurrentDirectory());
        }

        private static string GetTestDirectory()
        {
            return new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent.Parent.Parent.FullName + "/Source/Tests";
        }
    }
}
