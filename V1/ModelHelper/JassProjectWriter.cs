using System.IO;
using System.Text.Json.Nodes;
using vJassMainJBlueprint.V1.Config;
using vJassMainJBlueprint.V1.Model;

namespace vJassMainJBlueprint.V1.ModelHelper
{
    internal class JassProjectWriter
    {
        private static class VersionSpecification
        {
            public static readonly Dictionary<int, Func<int, JassProject, JsonObject>> VersionBuildActions = new()
                {
                    { 24090800, Version24090800 },
                };

            public static JsonObject Version24090800(int version, JassProject project)
            {
                var nodes = new JsonArray();
                foreach (var projectNode in project.Nodes)
                {
                    var node = new JsonObject
                    {
                        ["x"] = projectNode.X,
                        ["y"] = projectNode.Y,
                        ["width"] = projectNode.Width,
                        ["height"] = projectNode.Height,
                        ["sourceFilePath"] = projectNode.SourceFilePath,
                    };

                    if (projectNode.ImageBase64String != null)
                    {
                        node["imageBase64"] = projectNode.ImageBase64String;
                    }

                    nodes.Add(node);
                }

                return new JsonObject
                {
                    ["version"] = version,
                    ["name"] = project.Name,
                    ["author"] = project.Author,
                    ["width"] = project.Width,
                    ["height"] = project.Height,
                    ["items"] = nodes,
                };
            }
        }

        public static void Write(JassProject project, string filePath)
        {
            var json = VersionSpecification.VersionBuildActions[JassProjectSpecification.NewestVersion].Invoke(JassProjectSpecification.NewestVersion, project);

            using var writer = new StreamWriter(filePath);

            WriteJsonHeader(json, writer);
            WriteImportData(project, writer);
        }

        private static void WriteJsonHeader(JsonObject json, StreamWriter writer)
        {
            var jsonString = json.ToString();
            var lines = jsonString.Split(Environment.NewLine);
            foreach (var line in lines)
            {
                writer.WriteLine(JassProjectSpecification.JsonPrefix + line);
            }
        }

        private static void WriteImportData(JassProject project, StreamWriter writer)
        {
            foreach (var projectNode in project.Nodes)
            {
                if (projectNode.SourceFilePath == null || Path.GetExtension(projectNode.SourceFilePath) != ".j")
                {
                    continue;
                }

                // 상대 경로인 경우 선행되는 '..\' 를 제거
                if (projectNode.SourceFilePath.StartsWith("..\\"))
                {
                    writer.WriteLine($"//! import \"{projectNode.SourceFilePath[3..].Replace('\\', '/')}\"");
                }
                else
                {
                    writer.WriteLine($"//! import \"{projectNode.SourceFilePath.Replace('\\', '/')}\"");
                }
            }
        }
    }
}
