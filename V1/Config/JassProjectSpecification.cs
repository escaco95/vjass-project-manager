namespace vJassMainJBlueprint.V1.Config
{
    /// <summary>
    /// 전체 프로젝트의 스펙을 정의합니다.
    /// </summary>
    public static class JassProjectSpecification
    {
        /// <summary>
        /// 현재 어플리케이션이 지원하는 최신 버전입니다.
        /// </summary>
        public const int NewestVersion = 24090800;

        /// <summary>
        /// Jass Project 파일에서 Json 데이터를 구분하는 접두사입니다.
        /// </summary>
        public const string JsonPrefix = "//@";

        /// <summary>
        /// 어플리케이션에서 생성되는 프로젝트 이름 기본값입니다.
        /// </summary>
        public const string DefaultProjectName = "이름 없는 프로젝트";

        /// <summary>
        /// 어플리케이션에서 생성되는 프로젝트 제작자 기본값입니다.
        /// </summary>
        public const string DefaultProjectAuthor = "알 수 없는 제작자";

        /// <summary>
        /// 어플리케이션에서 생성되는 프로젝트 구상도 너비 기본값입니다.
        /// </summary>
        public const int DefaultProjectWidth = 2000;

        /// <summary>
        /// 어플리케이션에서 생성되는 프로젝트 구상도 높이 기본값입니다.
        /// </summary>
        public const int DefaultProjectHeight = 2000;

        /// <summary>
        /// 어플리케이션에서 허용하는 프로젝트 크기의 최소값입니다.
        /// </summary>
        public const int MinProjectSize = 1000;

        /// <summary>
        /// 어플리케이션에서 허용하는 프로젝트 크기의 최대값입니다.
        /// </summary>
        public const int MaxProjectSize = 5000;

        /// <summary>
        /// 어플리케이션에서 허용하는 최소 이동/크기 조정 거리 단위입니다.
        /// </summary>
        public const int SnapThreshold = 10;

        /// <summary>
        /// 어플리케이션에서 허용하는 최소 이동/크기 조정 거리 단위로 값을 정렬합니다.
        /// </summary>
        /// <param name="value">정렬할 값</param>
        /// <returns>정렬된 값</returns>
        public static int Snap(int value) => value / SnapThreshold * SnapThreshold;

        /// <summary>
        /// 어플리케이션에서 허용하는 프로젝트 크기입니다.
        /// </summary>
        public static bool IsAllowedProjectSize(int width, int height) => width >= MinProjectSize && width <= MaxProjectSize && height >= MinProjectSize && height <= MaxProjectSize;
    }
}
