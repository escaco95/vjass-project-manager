using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace vJassMainJBlueprint.Utils
{
    internal class MenuItemRecentFileHelper
    {
        private const int MaxRecentFiles = 10;
        private static readonly string RecentFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.recent.json");
        private static readonly List<string> CachedRecentFilePath = [];
        private static bool IsCached = false;

        public static void Apply(Window window, Func<List<MenuItem>> menuItemSupplier, Action<string> childClickAction)
        {
            if (window == null) return;

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
                // 캐싱된 적 없다면 저장하지 않음
                if (!IsCached) return;

                Save();
            };
        }

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
                File.WriteAllText(RecentFilePath + ".temp", json);

                if (File.Exists(RecentFilePath))
                {
                    File.Replace(RecentFilePath + ".temp", RecentFilePath, null);
                }
                else
                {
                    File.Move(RecentFilePath + ".temp", RecentFilePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save recent files: {ex.Message}");
            }
            finally
            {
                if (File.Exists(RecentFilePath + ".temp"))
                {
                    File.Delete(RecentFilePath + ".temp");
                }
            }
        }

        private static void Populate(MenuItem parentMenuItem, Action<string> childClickAction)
        {
            CacheRecentFilePath();

            parentMenuItem.Items.Clear();

            if (CachedRecentFilePath.Count == 0)
            {
                parentMenuItem.Items.Add(CreateNoItemsMenuItem());
            }
            else
            {
                int index = 1;
                foreach (var recentFile in CachedRecentFilePath)
                {
                    var menuItem = CreateRecentFileMenuItem(recentFile, index, childClickAction);
                    parentMenuItem.Items.Add(menuItem);
                    index++;
                }
            }
        }

        private static MenuItem CreateNoItemsMenuItem()
        {
            return new MenuItem
            {
                Header = "(항목 없음)",
                IsEnabled = false
            };
        }

        private static MenuItem CreateRecentFileMenuItem(string filePath, int index, Action<string> childClickAction)
        {
            var menuItem = new MenuItem
            {
                Header = $"{index} {Path.GetFileName(filePath)} ({Path.GetDirectoryName(filePath)})",
            };

            menuItem.Click += (sender, e) =>
            {
                if (sender is MenuItem clickedMenuItem)
                {
                    childClickAction(filePath);
                }
            };

            return menuItem;
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
