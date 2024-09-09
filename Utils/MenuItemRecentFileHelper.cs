using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace vJassMainJBlueprint.Utils
{
    internal class MenuItemRecentFileHelper
    {
        public static void Apply(Window window, Func<List<MenuItem>> menuItemSupplier, Action<string> childClickAction)
        {
            menuItemSupplier.Invoke().ForEach(menuItem =>
            {
                if (menuItem is not MenuItem parentMenuItem) return;
                parentMenuItem.SubmenuOpened += (menuSender, menuArgs) =>
                {
                    Populate(parentMenuItem, childClickAction);
                };
            });

            window.Closing += (sender, e) =>
            {
                Save();
            };
        }

        private const int MaxRecentFiles = 10;
        private static readonly string RecentFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.recent.json");
        private static readonly List<string> CachedRecentFilePath = [];
        private static bool IsCached = false;

        public static void Touch(string filePath)
        {
            CacheRecentFilePath();

            CachedRecentFilePath.Remove(filePath);
            CachedRecentFilePath.Insert(0, filePath);

            if (CachedRecentFilePath.Count > MaxRecentFiles)
            {
                CachedRecentFilePath.RemoveAt(CachedRecentFilePath.Count - 1);
            }
        }

        private static void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(CachedRecentFilePath);
                File.WriteAllText(RecentFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save recent files: {ex.Message}");
            }
        }

        private static void Populate(MenuItem parentMenuItem, Action<string> childClickAction)
        {
            CacheRecentFilePath();

            parentMenuItem.Items.Clear();

            if (CachedRecentFilePath.Count == 0)
            {
                parentMenuItem.Items.Add(new MenuItem
                {
                    Header = "(항목 없음)",
                    IsEnabled = false
                });
            }
            else
            {
                int index = 1;
                foreach (var recentFile in CachedRecentFilePath)
                {
                    var menuItem = new MenuItem
                    {
                        Header = $"{index++} {Path.GetFileName(recentFile)} ({Path.GetDirectoryName(recentFile)})",
                    };

                    menuItem.Click += (sender, e) =>
                    {
                        if (sender is MenuItem clickedMenuItem)
                        {
                            childClickAction(recentFile);
                        }
                    };

                    parentMenuItem.Items.Add(menuItem);
                }
            }
        }

        private static void CacheRecentFilePath()
        {
            if (IsCached) return;

            if (!File.Exists(RecentFilePath))
            {
                IsCached = true;
                return;
            }

            try
            {
                var json = File.ReadAllText(RecentFilePath);

                if (JsonSerializer.Deserialize<List<string>>(json) is not List<string> loadedRecentFiles) return;

                CachedRecentFilePath.AddRange(loadedRecentFiles);
                IsCached = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to cache recent files: {ex.Message}");
            }
        }
    }
}
