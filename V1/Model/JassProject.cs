using vJassMainJBlueprint.V1.Config;

namespace vJassMainJBlueprint.V1.Model
{
    public readonly record struct JassProject(string Name, string Author, int Width, int Height,  JassProject.Node[] Nodes)
    {
        /// <summary>
        /// 프로젝트의 기본값입니다.
        /// </summary>
        public static readonly JassProject Default = new(
            JassProjectSpecification.DefaultProjectName,
            JassProjectSpecification.DefaultProjectAuthor,
            JassProjectSpecification.DefaultProjectWidth,
            JassProjectSpecification.DefaultProjectHeight,
            []);

        public readonly string Name { get; } = Name;
        public readonly string Author { get; } = Author;
        public readonly int Width { get; } = Width;
        public readonly int Height { get; } = Height;
        public readonly Node[] Nodes { get; } = Nodes;

        public readonly record struct Node(int X, int Y, int Width, int Height, string SourceFilePath, string? ImageBase64String)
        {
            public readonly int X { get; } = X;
            public readonly int Y { get; } = Y;
            public readonly int Width { get; } = Width;
            public readonly int Height { get; } = Height;
            public readonly string SourceFilePath { get; } = SourceFilePath;
            public readonly string? ImageBase64String { get; } = ImageBase64String;
        }
    }
}
