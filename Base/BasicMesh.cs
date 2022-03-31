using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PISilnik.Base.Interfaces;
using PISilnik.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PISilnik.Base
{
    public class BasicMesh : IMesh
    {
        private List<GeometricVertex> geometricVertices;
        private int VAO;
        private int VBO;
        public string Name { get; set; }
        public int ElementVerticesCount { get; private set; }
        public List<GeometricVertex> GeometricVertices {
            get => geometricVertices;
            set {
                geometricVertices = value;
                Update();
            }
        }
        public List<TextureCoordinate> TextureCoordinates { get; set; }
        public List<NormalVertex> NormalVertices { get; set; }
        public List<PolygonGroup> PolygonGroups { get; set; }
        public Dictionary<string, Material> Materials { get; set; }
        public int TilingX { get; set; }
        public int TilingY { get; set; }

        private bool _initialized = false;

        public BasicMesh(string meshName)
        {
            geometricVertices = new();

            TextureCoordinates = new();
            NormalVertices = new();
            PolygonGroups = new();

            TilingX = 1;
            TilingY = 1;

            ElementVerticesCount = 0;

            Name = meshName;

            Materials = new();

            VAO = 0;
            VBO = 0;
        }

        public BasicMesh Clone()
        {
            BasicMesh cloneMesh = new(Name);
            cloneMesh.TextureCoordinates = new(TextureCoordinates);
            cloneMesh.NormalVertices = new(NormalVertices);
            cloneMesh.PolygonGroups = new(PolygonGroups);
            cloneMesh.Materials = new(Materials);
            cloneMesh.GeometricVertices = new(GeometricVertices);
            cloneMesh.TilingX = TilingX;
            cloneMesh.TilingY = TilingY;

            return cloneMesh;
        }
        public void InitializeGLBindings()
        {
            if (_initialized) return;

            _initialized = true;

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            ushort componentsLength = 11;

            int vertexLocation = Shader.BasicShader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, componentsLength * sizeof(float), 0);

            int texCoordLocation = Shader.BasicShader.GetAttribLocation("aTexCoords");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, componentsLength * sizeof(float), 3 * sizeof(float));

            int normalLocation = Shader.BasicShader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, componentsLength * sizeof(float), 5 * sizeof(float));

            int tangentLocation = Shader.BasicShader.GetAttribLocation("aTangent");
            GL.EnableVertexAttribArray(tangentLocation);
            GL.VertexAttribPointer(tangentLocation, 3, VertexAttribPointerType.Float, false, componentsLength * sizeof(float), 8 * sizeof(float));

            Update();
        }
        public BasicMesh() : this(string.Empty) { }
        private void Update()
        {
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            float[] verticesStream = PolygonGroups
                .SelectMany(pg => pg.Faces)
                .SelectMany(face => {

                    Vector3 edge1 = face.Vertices[1].GeometricVertex.Vec3 -
                        face.Vertices[0].GeometricVertex.Vec3;
                    Vector3 edge2 = face.Vertices[2].GeometricVertex.Vec3 -
                        face.Vertices[0].GeometricVertex.Vec3;

                    Vector2 deltaUV1 = face.Vertices[1].TextureCoordinate.Vec2 -
                        face.Vertices[0].TextureCoordinate.Vec2;
                    Vector2 deltaUV2 = face.Vertices[2].TextureCoordinate.Vec2 -
                        face.Vertices[0].TextureCoordinate.Vec2;

                    float f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

                    Vector3 tangent = (f * (deltaUV2.Y * edge1 - deltaUV1.Y * edge2)).Normalized();
                    //Vector3 bitangent = (f * (-deltaUV2.X * edge1 + deltaUV1.X * edge2)).Normalized();

                    if (face.SmoothShadingGroup <= 0)
                    {
                        Vector3 flatNormal =
                            (
                                face.Vertices[0].NormalVertex.Vec3 +
                                face.Vertices[1].NormalVertex.Vec3 +
                                face.Vertices[2].NormalVertex.Vec3
                            ).Normalized();
                        return face.Vertices.SelectMany(vertex => new[] {
                            vertex.GeometricVertex.X,
                            vertex.GeometricVertex.Y,
                            vertex.GeometricVertex.Z,
                            vertex.TextureCoordinate.X * TilingX,
                            vertex.TextureCoordinate.Y * TilingY,
                            flatNormal.X,
                            flatNormal.Y,
                            flatNormal.Z,
                            tangent.X,
                            tangent.Y,
                            tangent.Z
                        });
                    } else
                    {
                        return face.Vertices.SelectMany(vertex => new[] {
                            vertex.GeometricVertex.X,
                            vertex.GeometricVertex.Y,
                            vertex.GeometricVertex.Z,
                            vertex.TextureCoordinate.X * TilingX,
                            vertex.TextureCoordinate.Y * TilingY,
                            vertex.NormalVertex.X,
                            vertex.NormalVertex.Y,
                            vertex.NormalVertex.Z,
                            tangent.X,
                            tangent.Y,
                            tangent.Z
                        });
                    }
                })
                .ToArray();

            GL.BufferData(BufferTarget.ArrayBuffer, verticesStream.Length * sizeof(float), verticesStream, BufferUsageHint.StaticDraw);

            ElementVerticesCount = PolygonGroups
                .SelectMany(pg => pg.Faces)
                .SelectMany(face => face.Vertices)
                .Select(vertex => vertex.GeometricVertex)
                .Count();
        }

        public void Use()
        {
            GL.BindVertexArray(VAO);
        }
    }
}
