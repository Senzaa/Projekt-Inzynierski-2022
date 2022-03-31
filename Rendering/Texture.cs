using OpenTK.Graphics.OpenGL4;
using PISilnik.Rendering.Generators;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace PISilnik.Rendering
{
    public class Texture
    {
        public readonly int Handle;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] Pixels { get; private set; }

        public Texture(int width, int height, byte[] pixels)
        {
            Handle = GL.GenTexture();
            Pixels = pixels;

            GL.BindTexture(TextureTarget.Texture2D, Handle);

            Width = width;
            Height = height;

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                width,
                height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                pixels
                );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }
        public Texture() : this(0, 0, Array.Empty<byte>()) { }

        public static Texture LoadFromFile(string path)
        {
            byte[] pixels;
            int width = 0, height = 0;
            using (Image<Rgba32> image = Image.Load<Rgba32>(path))
            {
                image.Mutate(x => x.Flip(FlipMode.Vertical));

                width = image.Width;
                height = image.Height;

                pixels = new byte[4 * image.Width * image.Height];

                image.ProcessPixelRows(accessor => {
                    int offset = 0;
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                        for (int x = 0; x < pixelRow.Length; x++)
                        {
                            Rgba32 pixel = pixelRow[x];
                            pixels[offset] = pixel.R;
                            pixels[++offset] = pixel.G;
                            pixels[++offset] = pixel.B;
                            pixels[++offset] = pixel.A;
                            offset++;
                        }
                    }
                });
            }
            return new(width, height, pixels);
        }

        public static Texture Empty(int width, int height, byte r = 255, byte g = 255, byte b = 255, byte a = 255)
        {
            byte[] pixels = new byte[4 * width * height];
            int i = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    pixels[i] = r;
                    pixels[++i] = g;
                    pixels[++i] = b;
                    pixels[++i] = a;
                    i++;
                }
            }
            return new(width, height, pixels);
        }

        public static float[] GeneratePerlinMap(
            int width, int height,
            float xOrigin = 1f, float yOrigin = 1f,
            float scale = 1.0f,
            float zSlice = 1f)
        {
            float[] pixels = new float[width * height];
            int i = 0;
            for (float x = 0; x < width; x++)
            {
                for (float y = 0; y < height; y++)
                {
                    float xCoord = xOrigin + x / width * scale;
                    float yCoord = yOrigin + y / height * scale;
                    pixels[i] = PerlinNoise.GetSample(xCoord, yCoord, zSlice);
                    i++;
                }
            }
            return pixels;
        }

        public void ApplyMask(
            float[] noiseData,
            Func<float, RgbaVector, RgbaVector> func,
            float intensity = 1f)
        {
            int x = 0;
            for (int i = 0; i < Pixels.Length; i++)
            {
                RgbaVector colorIn = new(
                        Pixels[i] / 255f,
                        Pixels[i+1] / 255f,
                        Pixels[i+2] / 255f,
                        Pixels[i+3] / 255f
                    );
                float noise = noiseData[x] * intensity;
                RgbaVector color = func(noise, colorIn);
                Pixels[i] = (byte)(color.R * 255);
                Pixels[++i] = (byte)(color.G * 255);
                Pixels[++i] = (byte)(color.B * 255);
                Pixels[++i] = (byte)(color.A * 255);
                x++;
            }

            UpdatePixels(Pixels);
        }

        public void UpdatePixels(byte[] newPixels)
        {
            Pixels = newPixels;

            GL.BindTexture(TextureTarget.Texture2D, Handle);

            GL.TexSubImage2D(
                TextureTarget.Texture2D,
                0,
                0,
                0,
                Width,
                Height,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                Pixels
                );
        }

        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    }
}
