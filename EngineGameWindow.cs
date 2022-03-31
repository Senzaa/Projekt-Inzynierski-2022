using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PISilnik.Base;
using PISilnik.Base.Interfaces;
using PISilnik.Base.Physics;
using PISilnik.Rendering;
using PISilnik.Rendering.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PISilnik
{
    public class EngineGameWindow : GameWindow
    {
        private readonly IObjectsMaster[] ObjectsMasters;
        public EngineGameWindow(
            GameWindowSettings? gameWindowSettings = null,
            NativeWindowSettings? nativeWindowSettings = null) :
            base(gameWindowSettings ?? GameWindowSettings.Default,
                nativeWindowSettings ?? new() { StartVisible = false })
        {
            ObjectsMasters = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IObjectsMaster).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                .Select(type => (IObjectsMaster)Activator.CreateInstance(type)!)
                .ToArray();
        }

        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            base.OnLoad();

            Engine.ObjectsQueue.InitializeBasicObjects();

            Debug.WriteLine("OnLoad Complete");
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            for (int i = 0; i < ObjectsMasters.Length; i++)
                ObjectsMasters[i].Render(Engine.ObjectsQueue.RenderQueue, args.Time);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (IsFocused)
            {
                Engine.MainCamera.CameraSpeed = KeyboardState.IsKeyDown(Keys.LeftShift) ? 10f : Engine.MainCamera.DefaultCameraSpeed;
                if (KeyboardState.IsKeyDown(Keys.W))
                    Engine.MainCamera.Position += Engine.MainCamera.Front * Engine.MainCamera.CameraSpeed * (float)args.Time;

                if (KeyboardState.IsKeyDown(Keys.S))
                    Engine.MainCamera.Position -= Engine.MainCamera.Front * Engine.MainCamera.CameraSpeed * (float)args.Time;

                if (KeyboardState.IsKeyDown(Keys.A))
                    Engine.MainCamera.Position -= Engine.MainCamera.Right * Engine.MainCamera.CameraSpeed * (float)args.Time;

                if (KeyboardState.IsKeyDown(Keys.D))
                    Engine.MainCamera.Position += Engine.MainCamera.Right * Engine.MainCamera.CameraSpeed * (float)args.Time;

                if (KeyboardState.IsKeyDown(Keys.Space))
                    Engine.MainCamera.Position += Vector3.UnitY * Engine.MainCamera.CameraSpeed * (float)args.Time;

                if (KeyboardState.IsKeyDown(Keys.LeftControl))
                    Engine.MainCamera.Position -= Vector3.UnitY * Engine.MainCamera.CameraSpeed * (float)args.Time;

                if (MouseState.IsButtonDown(MouseButton.Right))
                {
                    if (!MouseState.WasButtonDown(MouseButton.Right))
                        CursorGrabbed = true;
                    Engine.MainCamera.Yaw += (MouseState.X - MouseState.PreviousPosition.X) * Engine.MainCamera.Sensitivity;
                    Engine.MainCamera.Pitch -= (MouseState.Y - MouseState.PreviousPosition.Y) * Engine.MainCamera.Sensitivity;
                }
                else if (MouseState.WasButtonDown(MouseButton.Right))
                {
                    CursorGrabbed = false;
                    CursorVisible = true;
                }
            }

            Physics.Timestep();

            for (int i = 0; i < ObjectsMasters.Length; i++)
                ObjectsMasters[i].Update(Engine.ObjectsQueue.UpdateQueue, args.Time);
        }

        protected override void OnUnload()
        {
            base.OnUnload();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);

            Engine.MainCamera.AspectRatio = Size.X / (float)Size.Y;
        }
        protected override void Dispose(bool disposing)
        {
            Console.WriteLine($"Dispose called with value: {disposing}");
            if (disposing)
                base.Dispose(true);
        }
    }
}
