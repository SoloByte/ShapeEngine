using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.Lib;
using ShapeEngine.Random;
using ShapeEngine.Screen;
using System.Net.NetworkInformation;
using System.Numerics;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.UI;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;

namespace Examples.Scenes.ExampleScenes
{
    public class CameraAreaDrawExample : ExampleScene
    {
        Font font;
        Vector2 movementDir = new();
        Rect universe = new(new Vector2(0f), new Vector2(10000f), new Vector2(0.5f));
        List<Star> stars = new();
        private List<Star> drawStars = new();
        
        private Ship ship = new(new Vector2(0f), 30f, ColorMedium, ColorLight, ColorHighlight1);
        private Ship ship2 = new(new Vector2(100, 0), 30f, ColorMedium, ColorHighlight2, ColorHighlight3);
        private Ship currentShip;
        private uint prevCameraTweenID = 0;
        private InputAction iaChangeCameraTarget;

        private readonly ShapeCamera camera;
        private readonly CameraFollowerSingle follower;
        public CameraAreaDrawExample()
        {
            Title = "Camera Area Draw Example";

            font = GAMELOOP.GetFont(FontIDs.JetBrains);

            camera = new();
            GenerateStars(ShapeRandom.randI(15000, 30000));
            follower = new(ship.Speed * 1.1f, 200, 400);
            camera.Follower = follower;
            UpdateFollower(GAMELOOP.UI.Area.Size.Min());
            
            currentShip = ship;

            var changeCameraTargetKB = new InputTypeKeyboardButton(ShapeKeyboardButton.B);
            var changeCameraTargetGP = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
            var changeCameraTargetMB = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
            iaChangeCameraTarget = new(changeCameraTargetKB, changeCameraTargetGP, changeCameraTargetMB);

        }

        private void UpdateFollower(float size)
        {
            var minBoundary = 0.12f * size;
            var maxBoundary = 0.25f * size;
            var boundary = new Vector2(minBoundary, maxBoundary) * camera.ZoomFactor;
            follower.Speed = ship.Speed * 1.1f;
            follower.BoundaryDis = new(boundary);
        }
        private void GenerateStars(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Vector2 pos = universe.GetRandomPointInside();

                //ChanceList<float> sizes = new((45, 1.5f), (25, 2f), (15, 2.5f), (10, 3f), (3, 3.5f), (2, 4f));
                float size = ShapeRandom.randF(1.5f, 3f);// sizes.Next();
                Star star = new(pos, size);
                stars.Add(star);
            }
        }
        
        public override void Activate(IScene oldScene)
        {
            GAMELOOP.Camera = camera;
            follower.SetTarget(ship);
            currentShip = ship;
            UpdateFollower(GAMELOOP.UI.Area.Size.Min());
            // GAMELOOP.UseMouseMovement = false;
        }

        public override void Deactivate()
        {
            GAMELOOP.ResetCamera();
            // GAMELOOP.UseMouseMovement = true;
        }
        public override GameObjectHandler? GetGameObjectHandler()
        {
            return null;
        }
        public override void Reset()
        {
            GAMELOOP.ScreenEffectIntensity = 1f;
            camera.Reset();
            ship.Reset(new Vector2(0), 30f);
            ship2.Reset(new Vector2(100, 0), 30f);
            follower.SetTarget(ship);
            currentShip = ship;
            UpdateFollower(GAMELOOP.UI.Area.Size.Min());
            stars.Clear();
            GenerateStars(ShapeRandom.randI(15000, 30000));

        }
        protected override void HandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            if (iaChangeCameraTarget.State.Pressed)
            {
                camera.StopTweenSequence(prevCameraTweenID);
                CameraTweenZoomFactor zoomFactorOut = new(1f, 0.5f, 0.25f, TweenType.LINEAR);
                CameraTweenZoomFactor zoomFactorIn = new(0.5f, 1.25f, 0.75f, TweenType.LINEAR);
                CameraTweenZoomFactor zoomFactorFinal = new(1.25f, 1f, 0.25f, TweenType.LINEAR);
                prevCameraTweenID = camera.StartTweenSequence(zoomFactorOut, zoomFactorIn, zoomFactorFinal);
                
                if (currentShip == ship)
                {
                    currentShip = ship2;
                    follower.ChangeTarget(ship2, 1f);
                }
                else
                {
                    currentShip = ship;
                    follower.ChangeTarget(ship, 1f);
                }
            }

        }
        // private void HandleZoom(float dt)
        // {
        //     float zoomSpeed = 1f;
        //     int zoomDir = 0;
        //     if (IsKeyDown(KeyboardKey.KEY_Z)) zoomDir = -1;
        //     else if (IsKeyDown(KeyboardKey.KEY_X)) zoomDir = 1;
        //
        //     if (zoomDir != 0)
        //     {
        //         camera.Zoom(zoomDir * zoomSpeed * dt);
        //     }
        // }
        protected override void UpdateExample(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            UpdateFollower(ui.Area.Size.Min());
            // int gamepadIndex = GAMELOOP.CurGamepad?.Index ?? -1;
            iaChangeCameraTarget.Gamepad = GAMELOOP.CurGamepad;
            iaChangeCameraTarget.Update(dt);
            
            currentShip.Update(dt, camera.RotationDeg);

            drawStars.Clear();
            Rect cameraArea = game.Area;
            foreach (var star in stars)
            {
                if(cameraArea.OverlapShape(star.GetBoundingBox())) drawStars.Add(star);
            }
        }

        protected override void DrawGameExample(ScreenInfo game)
        {
            foreach (var star in drawStars)
            {
                star.Draw(new Color(20, 150, 150, 200));
            }
            ship.Draw();
            ship2.Draw();
            
            // Circle cameraBoundaryMin = new(camera.Position, camera.Follower.BoundaryDis.Min);
            // cameraBoundaryMin.DrawLines(2f, ColorHighlight3);
            // Circle cameraBoundaryMax = new(camera.Position, camera.Follower.BoundaryDis.Max);
            // cameraBoundaryMax.DrawLines(2f, ColorHighlight2);
        }
        // protected override void DrawGameUIExample(ScreenInfo ui)
        // {
        //     // var infoRect = GAMELOOP.UIRects.GetRect("center").ApplyMargins(0.025f, 0.025f, 0.01f, 0.95f);
        //     // DrawStarInfo(infoRect);
        // }

        protected override void DrawUIExample(ScreenInfo ui)
        {
            var rects = GAMELOOP.UIRects.GetRect("bottom center").SplitV(0.5f);
            DrawStarInfo(rects.top);
            DrawInputDescription(rects.bottom);

            
        }

        private void DrawStarInfo(Rect rect)
        {
            string infoText = $"Total Stars {stars.Count} | Drawn Stars {drawStars.Count} | Camera Size {camera.Area.Size.Round()}";
            font.DrawText(infoText, rect, 1f, new Vector2(0.5f, 0.5f), ColorHighlight3);
        }
        private void DrawInputDescription(Rect rect)
        {
            var curDevice = ShapeInput.CurrentInputDeviceType;
            // var curDeviceNoMouse = Input.CurrentInputDeviceNoMouse;
            string changeTargetText = iaChangeCameraTarget.GetInputTypeDescription(curDevice, true, 1, false);
            string moveText = ship.GetInputDescription(curDevice);
            string shipInfoText = $"{moveText} | Switch Ship {changeTargetText}";
            font.DrawText(shipInfoText, rect, 1f, new Vector2(0.5f, 0.5f), ColorLight);
        }
    }

}
