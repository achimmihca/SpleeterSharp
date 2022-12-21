using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SpleeterSharp
{
    public class SpleeterSharpTests
    {
        [Fact]
        public void TestSpleeterVoiceSeparation()
        {
            SpleeterSharpConfig.Create()
                .SetSpleeterCommand("python -m spleeter")
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
            spleeterParameters.OutputFolder = "OutputFiles";
            spleeterParameters.OutputFileCodec = "ogg";
            SpleeterResult spleeterResult = SpleeterUtils.Split(spleeterParameters);

            Assert.NotEmpty(spleeterResult.Output);
            Assert.Equal(0, spleeterResult.ExitCode);

            string expectedVocalFilePath = "OutputFiles/audio_example/vocals.ogg";
            string expectedInstrumentalFilePath = "OutputFiles/audio_example/accompaniment.ogg";
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
        public void TestSpleeterVoiceSeparationError()
        {
            SpleeterSharpConfig.Create()
                .SetSpleeterCommand("python -m spleeter")
                .SetIsWindows(true)
                .SetLogAction(text => Debug.WriteLine(text));

            ChangeToTestDirectory();

            SpleeterParameters spleeterParameters = new SpleeterParameters();
            spleeterParameters.InputFile = "InputFiles/audio_example.mp3";
            spleeterParameters.OutputFolder = "OutputFiles";
            spleeterParameters.OutputFileCodec = "FORCE_ERROR";
            SpleeterResult spleeterResult = SpleeterUtils.Split(spleeterParameters);

            Assert.NotEmpty(spleeterResult.Output);
            Assert.True(spleeterResult.ExitCode != 0);
            Assert.NotEmpty(spleeterResult.Errors);
            Assert.NotEmpty(spleeterResult.Errors[0]);
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
