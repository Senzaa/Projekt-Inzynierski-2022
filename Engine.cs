using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using BepuPhysics;
using BepuUtilities.Memory;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing;
using OpenTK.Windowing.Desktop;
using PISilnik.Base;
using PISilnik.Base.Interfaces;
using PISilnik.Base.Physics;
using PISilnik.Base.Physics.Interfaces;
using PISilnik.Rendering;
using PISilnik.Rendering.Interfaces;

namespace PISilnik
{
    public static class Engine
    {
        public static bool Initialized { get; private set; }
        private static EngineGameWindow? _gameWindow;
        public static EngineGameWindow GameWindow {
            get {
                if (Initialized)
                    return _gameWindow!;
                else
                    throw new InvalidOperationException("Engine is not initialized!");
            }
        }
        public static Camera MainCamera { get; private set; }

        public static readonly ObjectsQueue ObjectsQueue;
        static Engine()
        {
            MainCamera = new(Vector3.Zero, 0.0f);
            ObjectsQueue = new();
        }
        public static void Run()
        {
            if (Initialized)
                GameWindow.Run();
            else
                throw new InvalidOperationException("The game engine is not yet initialized.");
        }

        public static void Initialize(
                GameWindowSettings? gws = null,
                NativeWindowSettings? nws = null
            )
        {
            if (!Initialized)
            {
                Initialized = true;
                try
                {
                    Physics.Initialize();
                    _gameWindow = new(gws, nws);
                    MainCamera = new(Vector3.UnitZ * 3, GameWindow.Size.X / (float)GameWindow.Size.Y);
                } catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to initialize Game Window:{Environment.NewLine}{ex}");
                    throw;
                }
            } else
                throw new InvalidOperationException("Engine is already initialized");
        }
    }
}
