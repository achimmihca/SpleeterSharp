namespace SpleeterSharp
{
    public static class SpleeterUtils
    {
        public static SpleeterResult Split(SpleeterParameters spleeterParameters)
        {
            return SpleeterCommandLineRunner.Split(spleeterParameters);
        }
    }
}
