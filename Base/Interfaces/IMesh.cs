using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PISilnik.Rendering;
using PISilnik.Rendering.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PISilnik.Base.Interfaces
{
    public struct GeometricVertex
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Vector3 Vec3 => new(X, Y, Z);
        public GeometricVertex()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
    }

    public struct TextureCoordinate
    {
        public float X { get; set; }
        public float Y { get; set; }
        public Vector2 Vec2 => new(X, Y);
        public TextureCoordinate()
        {
            X = 0;
            Y = 0;
        }
    }

    public struct NormalVertex
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public Vector3 Vec3 => new(X, Y, Z);
        public NormalVertex()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
    }
    public struct Vertex
    {
        public GeometricVertex GeometricVertex { get; set; }
        public TextureCoordinate TextureCoordinate { get; set; }
        public NormalVertex NormalVertex { get; set; }
        public Vertex(
            GeometricVertex geometricVertex,
            TextureCoordinate textureCoordinate,
            NormalVertex normalVertex)
        {
            GeometricVertex = geometricVertex;
            TextureCoordinate = textureCoordinate;
            NormalVertex = normalVertex;
        }
    }

    public struct Material
    {
        public Vector3 Ambient { get; set; }
        public Vector3 Diffuse { get; set; }
        public Vector3 Specular { get; set; }
        public float Shininess { get; set; }
        public Material(Vector3 ambient, Vector3 diffuse, Vector3 specular, float shininess)
        {
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Shininess = shininess;
        }
        public Material() : this(
            Vector3.One,
            new(0.8f),
            new(0.5f),
            500f
            )
        { }
    }

    public struct PolygonalFace
    {
        public List<Vertex> Vertices { get; set; }
        public uint SmoothShadingGroup { get; set; }
        public string MaterialName { get; set; }

        public PolygonalFace(IEnumerable<Vertex> vertices, uint smoothShadingGroup = 0)
        {
            Vertices = vertices.ToList();
            SmoothShadingGroup = smoothShadingGroup;
            MaterialName = string.Empty;
        }

        public PolygonalFace() : this(new List<Vertex>()) { }
    }

    public struct PolygonGroup
    {
        public string GroupName { get; set; }
        public List<PolygonalFace> Faces { get; set; }

        public PolygonGroup(string groupName)
        {
            GroupName = groupName;
            Faces = new();
        }

        public PolygonGroup() : this(string.Empty) { }
    }
    public interface IMesh
    {
        string Name { get; set; }
        List<GeometricVertex> GeometricVertices { get; set; }
        List<TextureCoordinate> TextureCoordinates { get; set; }
        List<NormalVertex> NormalVertices { get; set; }
        List<PolygonGroup> PolygonGroups { get; set; }
        Dictionary<string, Material> Materials { get; set; }
        int ElementVerticesCount { get; }
        int TilingX { get; set; }
        int TilingY { get; set; }
        void InitializeGLBindings();
        void Use();
    }
}
