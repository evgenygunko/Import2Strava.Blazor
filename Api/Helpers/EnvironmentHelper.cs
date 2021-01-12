namespace Api.Helpers
{
    public static class EnvironmentHelper
    {
        public static bool UseMockData
        {
            get
            {
                bool useMockData;
                if (!bool.TryParse(System.Environment.GetEnvironmentVariable("UseMockData"), out useMockData))
                {
                    useMockData = false;
                }

                return useMockData;
            }
        }
    }
}
