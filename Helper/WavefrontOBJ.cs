using OpenTK.Mathematics;
using PISilnik.Base;
using PISilnik.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PISilnik.Helper
{
    public class WavefrontOBJ
    {
        public readonly string OBJFilePath;
        public readonly string MTLFilePath;

        public List<BasicMesh> Meshes = new();
        public WavefrontOBJ(string objFilePath)
        {
            OBJFilePath = objFilePath;

            MTLFilePath = @$"{Directory.GetParent(OBJFilePath)}{Path.DirectorySeparatorChar}{
                File.ReadLines(OBJFilePath)
                .FirstOrDefault(line => line.StartsWith("mtllib "))
                ?[7..] ?? Path.GetFileNameWithoutExtension(OBJFilePath) + ".mtl"
                }";

            BasicMesh CurrentMesh = new();
            Dictionary<string, Material> Materials = new();
            string CurrentMaterial = string.Empty;
            PolygonGroup CurrentPolygonGroup = new("Group.000");
            uint CurrentShadingGroup = 0;

            if (!string.IsNullOrEmpty(MTLFilePath) &&
                File.Exists(MTLFilePath))
            {
                Material mat = new();
                string materialName = string.Empty;
                foreach (string line in File.ReadLines(MTLFilePath))
                {
                    if (line.StartsWith("newmtl "))
                    {
                        if (!string.IsNullOrEmpty(materialName))
                            Materials.Add(materialName, mat);
                        mat = new();
                        materialName = line[7..];
                    }
                    else if (line.StartsWith("Ns "))
                        mat.Shininess = float.Parse(line[3..], NumberStyles.Any, CultureInfo.InvariantCulture);
                    else if (line.StartsWith('K') && line.Length > 3)
                    {
                        string[] components = line[3..].Split();
                        if (components.Length >= 3)
                        {
                            Vector3 componentVec = new(
                                    float.Parse(components[0], NumberStyles.Any, CultureInfo.InvariantCulture),
                                    float.Parse(components[1], NumberStyles.Any, CultureInfo.InvariantCulture),
                                    float.Parse(components[2], NumberStyles.Any, CultureInfo.InvariantCulture)
                                );
                            if (line[1] == 'a')
                                mat.Ambient = componentVec;
                            else if (line[2] == 'd')
                                mat.Diffuse = componentVec;
                            else if (line[3] == 's')
                                mat.Specular = componentVec;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(materialName))
                    Materials.Add(materialName, mat);
                CurrentMesh.Materials = Materials;
            }

            foreach (string line in File.ReadLines(OBJFilePath))
            {
                try
                {
                    if (line.StartsWith("o"))
                    {
                        if (CurrentMesh.GeometricVertices.Count > 0)
                            Meshes.Add(CurrentMesh);
                        CurrentMesh = new(line.Length > 2 ? line[2..] : $"Mesh.{Meshes.Count:000}");
                        CurrentMesh.Materials = Materials;
                    }
                    else if (line.StartsWith("g"))
                    {
                        if (CurrentPolygonGroup.Faces.Count > 0)
                            CurrentMesh.PolygonGroups.Add(CurrentPolygonGroup);
                        CurrentPolygonGroup = new(line.Length > 2 ? line[2..] : $"Group.{CurrentMesh.PolygonGroups.Count:000}");
                    }
                    else if (line.StartsWith("v "))
                    {
                        string[] components = line[2..].Split();
                        if (components.Length >= 3)
                        {
                            CurrentMesh.GeometricVertices.Add(new()
                            {
                                X = float.Parse(components[0], NumberStyles.Any, CultureInfo.InvariantCulture),
                                Y = float.Parse(components[1], NumberStyles.Any, CultureInfo.InvariantCulture),
                                Z = float.Parse(components[2], NumberStyles.Any, CultureInfo.InvariantCulture)
                            });
                        }
                    }
                    else if (line.StartsWith("vt "))
                    {
                        string[] components = line[3..].Split();
                        if (components.Length >= 2)
                        {
                            CurrentMesh.TextureCoordinates.Add(new()
                            {
                                X = float.Parse(components[0], NumberStyles.Any, CultureInfo.InvariantCulture),
                                Y = float.Parse(components[1], NumberStyles.Any, CultureInfo.InvariantCulture)
                            });
                        }
                    }
                    else if (line.StartsWith("vn "))
                    {
                        string[] components = line[3..].Split();
                        if (components.Length >= 3)
                        {
                            CurrentMesh.NormalVertices.Add(new()
                            {
                                X = float.Parse(components[0], NumberStyles.Any, CultureInfo.InvariantCulture),
                                Y = float.Parse(components[1], NumberStyles.Any, CultureInfo.InvariantCulture),
                                Z = float.Parse(components[2], NumberStyles.Any, CultureInfo.InvariantCulture)
                            });
                        }
                    }
                    else if (line.StartsWith("usemtl ") && line.Length > 7)
                        CurrentMaterial = line[7..];
                    else if (line.StartsWith("s ") && line.Length > 2)
                        CurrentShadingGroup = line[2..] == "off" ? 0 : uint.Parse(line[2..], NumberStyles.Any, CultureInfo.InvariantCulture);
                    else if (line.StartsWith("f "))
                    {
                        PolygonalFace polyFace = new();
                        polyFace.SmoothShadingGroup = CurrentShadingGroup;
                        polyFace.MaterialName = CurrentMaterial;
                        string[] verticesData = line[2..].Split();
                        foreach (string vertexData in verticesData)
                        {
                            string[] components = vertexData.Split('/');
                            if (components.Length > 0)
                            {
                                if (int.TryParse(components[0], NumberStyles.Any, CultureInfo.InvariantCulture, out int vertexIndex))
                                {
                                    Vertex vertex = new();

                                    if (vertexIndex >= 0)
                                        vertex.GeometricVertex = CurrentMesh.GeometricVertices[vertexIndex - 1];
                                    else
                                        vertex.GeometricVertex = CurrentMesh.GeometricVertices[CurrentMesh.GeometricVertices.Count - 1 + vertexIndex];

                                    if (components.Length > 1 && components[1].Length > 0)
                                    {
                                        if (int.TryParse(components[1], NumberStyles.Any, CultureInfo.InvariantCulture, out int texCoordIndex))
                                        {
                                            if (texCoordIndex >= 0)
                                                vertex.TextureCoordinate = CurrentMesh.TextureCoordinates[texCoordIndex - 1];
                                            else
                                                vertex.TextureCoordinate = CurrentMesh.TextureCoordinates[CurrentMesh.TextureCoordinates.Count - 1 + texCoordIndex];
                                        }
                                    }
                                    if (components.Length > 2 && components[2].Length > 0)
                                    {
                                        if (int.TryParse(components[2], NumberStyles.Any, CultureInfo.InvariantCulture, out int normalVertexIndex))
                                        {
                                            if (normalVertexIndex >= 0)
                                                vertex.NormalVertex = CurrentMesh.NormalVertices[normalVertexIndex - 1];
                                            else
                                                vertex.NormalVertex = CurrentMesh.NormalVertices[CurrentMesh.NormalVertices.Count - 1 + normalVertexIndex];
                                        }
                                    }
                                    if (components.Length > 3)
                                        throw new Exception("Error: quads and n-gons are not yet supported.");

                                    polyFace.Vertices.Add(vertex);
                                }
                                else
                                    throw new Exception($"Invalid face geometric vertex component: {components[0]}");
                            }
                            else
                                throw new Exception("Invalid face vertex data");
                        }
                        CurrentPolygonGroup.Faces.Add(polyFace);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to read data from line:{Environment.NewLine}{line}{Environment.NewLine}Error message:{Environment.NewLine}{ex}");
                }
            }

            if (CurrentPolygonGroup.Faces.Count > 0)
                CurrentMesh.PolygonGroups.Add(CurrentPolygonGroup);

            if (CurrentMesh.GeometricVertices.Count > 0)
                Meshes.Add(CurrentMesh);
        }
    }
}
