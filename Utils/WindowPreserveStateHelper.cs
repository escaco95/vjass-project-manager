using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace vJassMainJBlueprint.Utils
{
    internal class WindowPreserveStateHelper
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };

        private record WindowStateConfig(bool IsMaximized, double Left, double Top, double Width, double Height, bool Topmost);

        public static void Apply(Window window, string configFileName)
        {
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName);

            window.Loaded += (sender, e) =>
            {
                ApplyWindowState(window, configFilePath);
            };

            window.Closing += (sender, e) =>
            {
                SaveWindowState(window, configFilePath);
            };
        }

        private static void ApplyWindowState(Window whichWindow, string configFilePath)
        {
            if (!File.Exists(configFilePath))
                return;

            try
            {
                var json = File.ReadAllText(configFilePath);
                var config = JsonSerializer.Deserialize<WindowStateConfig>(json);

                if (config != null)
                {
                    whichWindow.Left = config.Left;
                    whichWindow.Top = config.Top;
                    whichWindow.Width = config.Width;
                    whichWindow.Height = config.Height;
                    whichWindow.WindowState = config.IsMaximized ? WindowState.Maximized : WindowState.Normal;
                    whichWindow.Topmost = config.Topmost;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to apply window state: {ex.Message}");
            }
        }

        public static void SaveWindowState(Window whichWindow, string configFilePath)
        {
            WindowStateConfig config = new(
                IsMaximized: whichWindow.WindowState == WindowState.Maximized,
                Left: whichWindow.Left,
                Top: whichWindow.Top,
                Width: whichWindow.Width,
                Height: whichWindow.Height,
                Topmost: whichWindow.Topmost
            );

            try
            {
                var json = JsonSerializer.Serialize(config, jsonSerializerOptions);
                File.WriteAllText(configFilePath + ".temp", json);

                if (File.Exists(configFilePath))
                {
                    File.Replace(configFilePath + ".temp", configFilePath, null);
                }
                else
                {
                    File.Move(configFilePath + ".temp", configFilePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save window state: {ex.Message}");
            }
            finally
            {
                if (File.Exists(configFilePath + ".temp"))
                {
                    File.Delete(configFilePath + ".temp");
                }
            }
        }
    }
}
