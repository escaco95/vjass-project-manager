using System.Diagnostics;
using System.Windows;

namespace vJassMainJBlueprint.Utils
{
    public static class ProcessHelper
    {
        public static void Open(string filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일을 여는 도중 오류가 발생했습니다: {ex.Message}");
            }
        }
    }
}
