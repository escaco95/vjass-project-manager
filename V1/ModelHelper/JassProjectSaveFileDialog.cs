using Microsoft.Win32;

namespace vJassMainJBlueprint.V1.ModelHelper
{
    internal class JassProjectSaveFileDialog
    {
        private static readonly SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "vJass Script files (*.j)|*.j|All files (*.*)|*.*", // 파일 필터 설정
            Title = "파일 저장" // 대화상자 제목 설정
        };

        /// <summary>
        /// Displays the save file dialog and returns the selected file path.
        /// </summary>
        /// <returns>The selected file path, or null if no file is selected.</returns>
        public static string? Show()
        {
            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }
            else
            {
                return null;
            }
        }
    }
}
