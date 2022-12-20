using System.Diagnostics;
using System.IO;
using System.Reflection;
using Xunit;

namespace SpleeterSharp
{
    public class MyTest
    {
        [Fact]
        public void DummyTest()
        {
            Assert.True(4 < 11);
        }

        [Fact]
        public void TestSpleeterVoiceSeparation()
        {
            Debug.WriteLine("current path: " + System.IO.Directory.GetCurrentDirectory());
            SpleeterSharpConfig.Create()
                .SetIsWindows(true)
                .SetLogAction(text => Debug.WriteLine(text));

            Directory.SetCurrentDirectory(GetTestDirectory());

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

            Assert.True(File.Exists("OutputFiles/audio_example/vocals.ogg"));
            Assert.True(File.Exists("OutputFiles/audio_example/accompaniment.ogg"));

            // TODO: Test SpleeterResult parses the written files from the process output
        }

        // TODO: Test SpleeterResult parses the errors from the process output

        private static string GetTestDirectory()
        {
            return new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent.Parent.Parent.FullName + "/Source/Tests";
        }
    }
}
