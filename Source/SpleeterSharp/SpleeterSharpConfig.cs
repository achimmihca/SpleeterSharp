using System;

namespace SpleeterSharp
{
    public class SpleeterSharpConfig
    {
        private static SpleeterSharpConfig config;
        public static SpleeterSharpConfig Config
        {
            get
            {
                if (config == null)
                {
                    throw new SpleeterSharpException("SpleeterSharp config not initialized");
                }
                return config;
            }
        }

        public static SpleeterSharpConfig Create()
        {
            config = new SpleeterSharpConfig();
            return config;
        }

        public bool IsWindows { get; private set; }
        public Action<string> LogAction { get; private set; }

        private SpleeterSharpConfig()
        {
        }

        public SpleeterSharpConfig SetIsWindows(bool isWindows)
        {
            IsWindows = isWindows;
            return this;
        }

        public SpleeterSharpConfig SetLogAction(Action<string> logAction)
        {
            LogAction = logAction;
            return this;
        }
    }
}
