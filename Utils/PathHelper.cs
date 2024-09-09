using System.IO;

namespace vJassMainJBlueprint.Utils
{
    public static class PathHelper
    {
        /// <summary>
        /// 주어진 경로를 절대 경로로 변환합니다.
        /// </summary>
        /// <param name="basePath">절대화 기준 경로. 파일 또는 폴더 경로입니다.</param>
        /// <param name="relativePath">변환 대상 경로</param>
        /// <returns>절대 경로</returns>
        public static string ConvertToAbsolutePath(string basePath, string relativePath)
        {
            return Path.GetFullPath(Path.Combine(basePath, relativePath));
        }

        /// <summary>
        /// 주어진 경로 목록을 지정한 경로를 기준으로 한 상대 경로로 변환합니다.
        /// </summary>
        /// <param name="basePath">상대화 기준 경로. 파일 또는 폴더 경로입니다.</param>
        /// <param name="paths">변환 대상 경로</param>
        /// <returns>원본,변환 결과 쌍. 변환할 수 없거나, 필요하지 않은 항목은 사전에 포함되지 않습니다.</returns>
        public static Dictionary<string, string> ConvertToRelativePaths(string basePath, List<string> paths)
        {
            Dictionary<string, string> result = [];

            foreach (string path in paths)
            {
                string relativePath = Path.GetRelativePath(basePath, path);

                // 변경사항이 없거나 변경 후 경로에 둘 이상의 ..\가 포함되어 있으면 소스 디렉토리 밖의 경로이므로 변환할 필요가 없습니다.
                if (relativePath == path || relativePath.Contains(@"..\..\")) continue;

                result.Add(path, relativePath);
            }

            return result;
        }
    }
}
