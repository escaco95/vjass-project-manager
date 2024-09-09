﻿using System.IO;
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
            json.ToString()
                // 줄 단위로 나눕니다
                .Split(Environment.NewLine)
                // 다른 데이터와 구분될 수 있도록 각 줄에 JassProjectSpecification.JsonPrefix를 추가합니다.
                .Select(line => JassProjectSpecification.JsonPrefix + line)
                // 파일에 작성합니다.
                .ToList()
                .ForEach(writer.WriteLine);
        }

        private static void WriteImportData(JassProject project, StreamWriter writer)
        {
            project.Nodes
                .Select(node => node.SourceFilePath)
                .Where(path => path != null)
                // J 파일이 아닌 경우 건너뜁니다.
                .Where(path => Path.GetExtension(path) == ".j")
                // C# 상대 경로 표기를 vJass Import 상대 경로 표기로 변경합니다. (절대 경로인 경우 변경 안 함)
                .Select(path => path.StartsWith("..\\") ? path[3..] : path)
                // C# 디렉토리 구분자를 vJass Import 디렉토리 구분자로 변경합니다.
                .Select(path => path.Replace('\\', '/'))
                // vJass Import 구문을 작성합니다.
                .Select(path => $"//! import \"{path}\"")
                // 모든 절차를 정상 통과한 목록을 파일에 작성합니다.
                .ToList()
                .ForEach(writer.WriteLine);
        }
    }
}
