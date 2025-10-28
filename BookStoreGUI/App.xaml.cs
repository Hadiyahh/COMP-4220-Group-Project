using System;
using System.IO;
using System.Windows;

namespace BookStoreGUI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            TryLoadDotEnv();
            AppDomain.CurrentDomain.SetData(
                "DataDirectory",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database"));

            base.OnStartup(e);
        }

        private static void TryLoadDotEnv()
        {
            try
            {
                var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
                {
                    var envPath = Path.Combine(dir.FullName, ".env");
                    if (!File.Exists(envPath)) continue;

                    foreach (var raw in File.ReadAllLines(envPath))
                    {
                        var line = raw.Trim();
                        if (line.Length == 0 || line.StartsWith("#")) continue;
                        var idx = line.IndexOf('=');
                        if (idx <= 0) continue;
                        var key = line.Substring(0, idx).Trim();
                        var val = line.Substring(idx + 1).Trim().Trim('"');
                        Environment.SetEnvironmentVariable(key, val, EnvironmentVariableTarget.Process);
                    }
                    break; 
                }
            }
            catch
            {
                //throw err somewhere else
            }
        }
    }
}
