﻿using System.Formats.Tar;
using Raylib_CsLo;
using ShapeEngine.Core;
using ShapeEngine.UI;
using System.Numerics;
using ShapeEngine.Lib;
using Examples.Scenes.ExampleScenes;
using Examples.UIElements;
using ShapeEngine.Core.Interfaces;
using ShapeEngine.Core.Structs;
using ShapeEngine.Screen;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Input;

namespace Examples.Scenes
{
    // public class UIBox
    // {
    //     public Rect Rect;
    //     //private List<UIBox> children = new();
    //
    //     public virtual void Update(float dt, Rect parentRect)
    //     {
    //
    //     }
    //     public virtual void Draw()
    //     {
    //         
    //     }
    // }
    
    //Info box
    //-General-
    //Reset
    //Fullscreen
    
    //-Scene-
    //Zoom
    //CRT Shader
    
    
    public class MainScene : IScene
    {
        //should display names/description of all examples
        //left half lists all examples vertical / right half displays short info

        private int curPageIndex = 0;
        private List<ExampleScene> examples = new();
        private List<ExampleSelectionButton> buttons = new();
        private UIElement curButton;
        private Font titleFont;

        private float tabChangeMouseWheelLockTimer = 0f;
        private InputActionLabel quitLabel;
        public MainScene()
        {
            for (var i = 0; i < 10; i++)
            {
                ExampleSelectionButton b = new ExampleSelectionButton();
                buttons.Add(b);
                b.WasSelected += OnButtonSelected;
            }
            curButton = buttons[0];
            examples.Add(new PolylineInflationExample());
            examples.Add(new AsteroidMiningExample());
            examples.Add(new GameObjectHandlerExample());
            examples.Add(new ScreenEffectsExample());
            examples.Add(new CameraGroupFollowExample());
            examples.Add(new ShipInputExample());
            
            examples.Add(new InputExample());
            examples.Add(new CameraExample());
            examples.Add(new CameraAreaDrawExample());
            examples.Add(new BouncyCircles());
            examples.Add(new DelaunayExample());
            
            examples.Add(new TextScalingExample());
            examples.Add(new TextWrapEmphasisExample());
            examples.Add(new WordEmphasisDynamicExample());
            examples.Add(new TextBoxExample()); 
            
            //examples.Add(new TextEmphasisExample());
            //examples.Add(new WordEmphasisStaticExample());
            //examples.Add(new TextWrapExample());
            //examples.Add(new TextRotationExample());
            //examples.Add(new PolylineCollisionExample());
            //examples.Add(new CCDExample());
            
            titleFont = GAMELOOP.FontDefault; // GAMELOOP.GetFont(GameloopExamples.FONT_IndieFlowerRegular);

            SetupButtons();

            var action = GAMELOOP.InputActionUICancel;// GAMELOOP.Input.GetAction(GameloopExamples.InputUICancelID);
            quitLabel = new(action, "Quit", GAMELOOP.FontDefault, ExampleScene.ColorHighlight3);
        }
        
        public void OnWindowSizeChanged(DimensionConversionFactors conversionFactors)
        {
            
        }

        public void OnWindowPositionChanged(Vector2 oldPos, Vector2 newPos)
        {
        }

        public void OnMonitorChanged(MonitorInfo newMonitor)
        {
        }

        public void OnGamepadConnected(ShapeGamepadDevice gamepad)
        {
        }

        public void OnGamepadDisconnected(ShapeGamepadDevice gamepad)
        {
        }

        public void OnInputDeviceChanged(InputDeviceType prevDeviceType, InputDeviceType curDeviceType)
        {
        }

        public void OnPauseChanged(bool paused){}
        
        public void HandleInput(float dt, Vector2 mousePosGame, Vector2 mousePosUI)
        {
            // if (Raylib.IsKeyPressed(KeyboardKey.KEY_ONE))
            // {
            //     if (IsWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST))
            //     {
            //         ClearWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);
            //     }
            //     else SetWindowState(ConfigFlags.FLAG_WINDOW_TOPMOST);
            // }
            // if (Raylib.IsKeyPressed(KeyboardKey.KEY_TWO))
            // {
            //     if (IsWindowState(ConfigFlags.FLAG_WINDOW_TRANSPARENT))
            //     {
            //         ClearWindowState(ConfigFlags.FLAG_WINDOW_TRANSPARENT);
            //     }
            //     else SetWindowState(ConfigFlags.FLAG_WINDOW_TRANSPARENT);
            // }
            // if (Raylib.IsKeyPressed(KeyboardKey.KEY_THREE))
            // {
            //     if (IsWindowState(ConfigFlags.FLAG_INTERLACED_HINT))
            //     {
            //         ClearWindowState(ConfigFlags.FLAG_INTERLACED_HINT);
            //     }
            //     else SetWindowState(ConfigFlags.FLAG_INTERLACED_HINT);
            // }
            // if (Raylib.IsKeyPressed(KeyboardKey.KEY_FOUR))
            // {
            //     if (IsWindowState(ConfigFlags.FLAG_WINDOW_MOUSE_PASSTHROUGH))
            //     {
            //         ClearWindowState(ConfigFlags.FLAG_WINDOW_MOUSE_PASSTHROUGH);
            //     }
            //     else SetWindowState(ConfigFlags.FLAG_WINDOW_MOUSE_PASSTHROUGH);
            // }
            // if (Raylib.IsKeyPressed(KeyboardKey.KEY_FIVE))
            // {
            //     if (IsWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED))
            //     {
            //         ClearWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            //     }
            //     else SetWindowState(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            // }
            // if (Raylib.IsKeyPressed(KeyboardKey.KEY_SIX))
            // {
            //     if (IsWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE))
            //     {
            //         ClearWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            //     }
            //     else SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            // }
            // if (Raylib.IsKeyPressed(KeyboardKey.KEY_SEVEN))
            // {
            //     if (IsWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED))
            //     {
            //         ClearWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
            //     }
            //     else SetWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
            // }
            // if (Raylib.IsKeyPressed(KeyboardKey.KEY_EIGHT))
            // {
            //     if (IsWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED))
            //     {
            //         ClearWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
            //     }
            //     else SetWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
            // }
            // if (Raylib.IsKeyPressed(KeyboardKey.KEY_NINE))
            // {
            //     if (IsWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED))
            //     {
            //         ClearWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
            //     }
            //     else SetWindowState(ConfigFlags.FLAG_WINDOW_UNFOCUSED);
            // }


            var cancelState = GAMELOOP.InputActionUICancel.Consume();
            if (cancelState is { Consumed: false, Pressed: true })
            {
                GAMELOOP.Quit();
            }

            var resetState = GAMELOOP.InputActionReset.Consume();
            if (resetState is { Consumed: false, Pressed: true })
            {
                for (int i = 0; i < examples.Count; i++)
                {
                    examples[i].Reset();
                }
                GAMELOOP.Camera.Reset();
                GAMELOOP.ResetCamera();
            }

            var maximizedState = GAMELOOP.InputActionMaximize.Consume();
            if (maximizedState is { Consumed: false, Pressed: true })
            { 
                GAMELOOP.Maximized = !GAMELOOP.Maximized;
            }

            var fullscreenState = GAMELOOP.InputActionFullscreen.Consume();
            if (fullscreenState is { Consumed: false, Pressed: true })
            { 
                GAMELOOP.Fullscreen = !GAMELOOP.Fullscreen;
            }

            var prevTabState = GAMELOOP.InputActionUIPrevTab.Consume();
            if (prevTabState is { Consumed: false, Pressed: true })
            {
                if (prevTabState.InputDeviceType == InputDeviceType.Mouse)
                {
                    if (tabChangeMouseWheelLockTimer <= 0)
                    {
                        PrevPage();
                        tabChangeMouseWheelLockTimer = 0.5f;
                    }
                }
                else
                {
                    PrevPage();
                    tabChangeMouseWheelLockTimer = 0f;
                }
                //PrevPage();
            }

            var nextTabState = GAMELOOP.InputActionUINextTab.Consume();
            if (nextTabState is { Consumed: false, Pressed: true })
            { 
                if (nextTabState.InputDeviceType == InputDeviceType.Mouse)
                {
                    if (tabChangeMouseWheelLockTimer <= 0)
                    {
                        NextPage();
                        tabChangeMouseWheelLockTimer = 0.5f;
                    }
                }
                else
                {
                    NextPage();
                    tabChangeMouseWheelLockTimer = 0f;
                }
                // NextPage();
            }

            var uiDownState = GAMELOOP.InputActionUIDown.Consume();
            if (uiDownState is { Consumed: false, Pressed: true })
            { 
                NextButton();
            }

            var uiUpState = GAMELOOP.InputActionUIUp.Consume();
            if (uiUpState is { Consumed: false, Pressed: true })
            { 
                PrevButton();
            }

            var nextMonitorState = GAMELOOP.InputActionNextMonitor.Consume();
            if (nextMonitorState is { Consumed: false, Pressed: true })
            { 
                GAMELOOP.NextMonitor();
            }
        }

        public void Update(float dt, float deltaSlow, ScreenInfo game, ScreenInfo ui)
        {
            if (tabChangeMouseWheelLockTimer > 0f)
            {
                tabChangeMouseWheelLockTimer -= dt;
                if (tabChangeMouseWheelLockTimer <= 0f) tabChangeMouseWheelLockTimer = 0f;
            }
            HandleInput(dt, game.MousePos, ui.MousePos);
            foreach (var b in buttons)
            {
                b.Update(dt, ui.MousePos);
            }
        }
        
        public void DrawGameUI(ScreenInfo ui)
        {
            
           
        }

        public void DrawUI(ScreenInfo ui)
        {
            var uiSize = ui.Area.Size;
            var start = uiSize * new Vector2(0.02f, 0.25f);
            var size = uiSize * new Vector2(0.45f, 0.05f);
            var gap = uiSize * new Vector2(0f, 0.07f);
            for (int i = 0; i < buttons.Count; i++)
            {
                var b = buttons[i];
                b.UpdateRect(start + gap * i, size, new Vector2(0f));
                b.Draw();
            }


            var text = "Shape Engine Examples";
            var titleRect = new Rect(uiSize * new Vector2(0.5f, 0.01f), uiSize * new Vector2(0.75f, 0.09f), new Vector2(0.5f, 0f));
            titleFont.DrawText(text, titleRect, 10, new(0.5f), ExampleScene.ColorLight);

            int pages = GetMaxPages();
            string prevName = GAMELOOP.InputActionUIPrevTab.GetInputTypeDescription(ShapeInput.CurrentInputDeviceType, true, 1, false);
            string nextName = GAMELOOP.InputActionUINextTab.GetInputTypeDescription(ShapeInput.CurrentInputDeviceType, true, 1, false);
            
            string pagesText = pages <= 1 ? "Page 1/1" : $"{prevName} <- Page #{curPageIndex + 1}/{pages} -> {nextName}";
            var pageRect = new Rect(uiSize * new Vector2(0.01f, 0.12f), uiSize * new Vector2(0.3f, 0.06f), new Vector2(0f, 0f));
            titleFont.DrawText(pagesText, pageRect, 4f, new(0f, 0.5f), ExampleScene.ColorHighlight2);

            Segment s = new(uiSize * new Vector2(0f, 0.22f), uiSize * new Vector2(1f, 0.22f));
            s.Draw(MathF.Max(4f * GAMELOOP.DevelopmentToScreen.AreaFactor, 0.5f), ExampleScene.ColorLight);

            var backRect = new Rect(uiSize * new Vector2(0.01f, 0.17f), uiSize * new Vector2(0.2f, 0.04f), new Vector2(0f, 0f));
            var curInputDevice = ShapeInput.CurrentInputDeviceTypeNoMouse;
            quitLabel.Draw(backRect, new Vector2(0f), curInputDevice, 1f);

            var infoArea = ui.Area.ApplyMargins(0.7f, 0.025f, 0.14f, 0.79f);
            var infoAreaRects = infoArea.SplitV(0.5f);
            float p = GAMELOOP.GetScreenPercentage();
            int pi = (int)MathF.Round(p * 100);
            titleFont.DrawText($"Window Focused: {Raylib.IsWindowFocused()} | [{pi}%]", infoAreaRects.top, 1f, new Vector2(1f, 1f), ExampleScene.ColorHighlight3);
            titleFont.DrawText($"Cursor On Screen: {ShapeLoop.CursorOnScreen}", infoAreaRects.bottom, 1f, new Vector2(1f, 1f), ExampleScene.ColorHighlight3);
            
            // var r = ui.Area.ApplyMargins(0.75f, 0.025f, 0.17f, 0.79f);
            // titleFont.DrawText($"Cursor On Screen: {ShapeLoop.CursorOnScreen}", r, 1f, new Vector2(1f, 1f), ExampleScene.ColorHighlight2);

            
            
            
            var centerRight = GAMELOOP.UIRects.GetRect("center right");
            var inputInfoRect = centerRight.ApplyMargins(0.25f, -0.025f, 0.15f, 0.55f);
            DrawInputInfoBox(inputInfoRect);
            // DrawInputDeviceInfo(ui.Area.ApplyMargins(0.7f, 0.01f, 0.7f, 0.01f));
        }

        
        // private void DrawInputDeviceInfo(Rect rect)
        // {
        //     if (Raylib.IsGamepadAvailable(0))
        //     {
        //         if (Raylib.IsGamepadButtonDown(0, GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_DOWN))
        //         {
        //             titleFont.DrawText("Gamepad 0 (A) is Down", rect, 1f, new Vector2(0.01f, 0.5f), RED);
        //             return;
        //         }
        //     }
        //     
        //     var infoRect = rect;
        //     var split = infoRect.SplitV(2);
        //     var deviceRect = split[0];
        //     var gamepadRect = split[1];
        //
        //     var lastUsedGamepad = ShapeInput.GamepadDeviceManager.LastUsedGamepad;
        //     if (lastUsedGamepad != null)
        //     {
        //         var deviceText = $"Used: {lastUsedGamepad.Name} | Buttons {lastUsedGamepad.UsedButtons.Count} | Axis {lastUsedGamepad.UsedAxis.Count}";
        //         titleFont.DrawText(deviceText, deviceRect, 1f, new Vector2(0.01f, 0.5f), RED);
        //     }
        //     else
        //     {
        //         titleFont.DrawText("No gamepad used", deviceRect, 1f, new Vector2(0.01f, 0.5f), RED);
        //     }
        //     
        //     
        //     // var deviceText = ShapeInput.GetCurInputDeviceGenericName();
        //     // titleFont.DrawText(deviceText, deviceRect, 1f, new Vector2(0.01f, 0.5f), RED);
        //     
        //     string gamepadText = "No Gamepad Connected";
        //     if (GAMELOOP.CurGamepad != null)
        //     {
        //         var gamepadIndex = GAMELOOP.CurGamepad.Index;
        //         gamepadText = $"Gamepad [{gamepadIndex}] Connected | Locked {GAMELOOP.CurGamepad.IsLocked()}";
        //     }
        //     
        //     titleFont.DrawText(gamepadText, gamepadRect, 1f, new Vector2(0.01f, 0.5f), GAMELOOP.CurGamepad != null ? RED : GRAY);
        // }
        
        private void OnButtonSelected(UIElement button)
        {
            if (curButton != button)
            {
                GAMELOOP.Cursor.TriggerEffect("scale");
                curButton.Deselect();
                curButton = button;
            }
        }
        private int GetCurButtonIndex()
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                var b = buttons[i];
                if (b.Selected) return i;
            }
            return -1;
        }
        private int GetVisibleButtonCount()
        {
            var count = 0;
            foreach (var b in buttons)
            {
                if (!b.Hidden) count++;
            }
            return count;
        }
        private int GetMaxPages()
        {
            if (buttons.Count <= 0 || examples.Count <= 0) return 1;
            int pages = (examples.Count - 1) / buttons.Count;
            if (pages < 1) return 1;
            else return pages + 1;
        }
        private void SetupButtons()
        {
            int startIndex = curPageIndex * buttons.Count;
            int endIndex = startIndex + buttons.Count;
            var buttonIndex = 0;
            for (int i = curPageIndex * buttons.Count; i < endIndex; i++)
            {
                var b = buttons[buttonIndex];
                buttonIndex++;
                if (i < examples.Count) b.SetScene(examples[i]);
                else b.SetScene(null);
            }

            buttons[0].Select();
        }
        private void NextButton()
        {
            int curButtonIndex = GetCurButtonIndex();
            if (curButtonIndex < 0) return;
            curButtonIndex++;
            int buttonCount = GetVisibleButtonCount();
            if (curButtonIndex >= buttonCount) curButtonIndex = 0;
            buttons[curButtonIndex].Select();
        }
        private void PrevButton()
        {
            int curButtonIndex = GetCurButtonIndex();
            if (curButtonIndex < 0) return;
            curButtonIndex--;
            int buttonCount = GetVisibleButtonCount();
            if (curButtonIndex < 0) curButtonIndex = buttonCount - 1;
            buttons[curButtonIndex].Select();
        }
        private void NextPage()
        {
            int maxPages = GetMaxPages();
            if (maxPages <= 1) return;

            curPageIndex++;
            if (curPageIndex >= maxPages) curPageIndex = 0;
            SetupButtons();
        }
        private void PrevPage()
        {
            int maxPages = GetMaxPages();
            if (maxPages <= 1) return;

            curPageIndex--;
            if (curPageIndex < 0) curPageIndex = maxPages - 1;
            SetupButtons();
        }

        private void DrawInputInfoBox(Rect area)
        {
            //area.DrawLinesDotted(3, 1f, ExampleScene.ColorMedium, LineCapType.Capped, 3);
            //area = area.ApplyMargins(0.05f, 0.05f, 0.01f, 0.01f);
            var curInputDevice = ShapeInput.CurrentInputDeviceType;
            if (curInputDevice == InputDeviceType.Mouse) curInputDevice = InputDeviceType.Keyboard;

            string fullscreenInputTypeName = GAMELOOP.InputActionFullscreen.GetInputTypeDescription(curInputDevice, true, 1, false);
            var fullscreenInfo = $"Fullscreen {fullscreenInputTypeName}";
            
            string crtInputTypeNamesPlus = GAMELOOP.InputActionCRTPlus.GetInputTypeDescription(curInputDevice, true, 1, false, false);
            string crtInputTypeNamesMinus = GAMELOOP.InputActionCRTMinus.GetInputTypeDescription(curInputDevice, true, 1, false, false);
            var crtInfo = $"CRT Shader [{crtInputTypeNamesPlus}|{crtInputTypeNamesMinus}]";
            
            string zoomInputTypeName = GAMELOOP.InputActionZoom.GetInputTypeDescription(ShapeInput.CurrentInputDeviceType, true, 1, false);
            var zoomInfo = $"Zoom Example {zoomInputTypeName}";
            
            string pauseInputTypeName = GAMELOOP.InputActionPause.GetInputTypeDescription(curInputDevice, true, 1, false);
            var pauseInfo = $"Pause Example {pauseInputTypeName}";
            
            string resetInputTypeName = GAMELOOP.InputActionReset.GetInputTypeDescription(curInputDevice, true, 1, false);
            var resetInfo = $"Reset Example {resetInputTypeName}";

            var rects = area.SplitV(5);

            var color = ExampleScene.ColorMedium;
            var alignement = new Vector2(1f, 0.05f);
            titleFont.DrawText(fullscreenInfo, rects[0], 1f, alignement, color);
            titleFont.DrawText(crtInfo, rects[1], 1f, alignement, color);
            titleFont.DrawText(resetInfo, rects[2], 1f, alignement, color);
            titleFont.DrawText(zoomInfo, rects[3], 1f, alignement, color);
            titleFont.DrawText(pauseInfo, rects[4], 1f, alignement, color);
        }
        
        private void DrawScreenInfoDebug(Rect uiArea)
        {
            var rightHalf = uiArea.ApplyMargins(0.6f, 0.025f, 0.25f, 0.025f);
            //rightHalf.DrawLines(2f, RED);

            List<string> infos = new();
            
            int monitor = Raylib.GetCurrentMonitor();
            infos.Add($"[{monitor}] Monitor Size: {Raylib.GetMonitorWidth(monitor)}|{Raylib.GetMonitorHeight(monitor)}");
            infos.Add($"Window(Screen) Size: {Raylib.GetScreenWidth()}|{Raylib.GetScreenHeight()}");
            infos.Add($"Render Size: {Raylib.GetRenderWidth()}|{Raylib.GetRenderHeight()}");
            infos.Add($"Scale DPI: {Raylib.GetWindowScaleDPI().X}|{Raylib.GetWindowScaleDPI().Y}");
            infos.Add($"HIGH Dpi: {Raylib.IsWindowState(ConfigFlags.FLAG_WINDOW_HIGHDPI)}");

            var rects = rightHalf.SplitV(infos.Count);

            for (var i = 0; i < infos.Count; i++)
            {
                string infoText = infos[i];
                var rect = rects[i];
                titleFont.DrawText(infoText, rect, 1f, new Vector2(0.95f, 0.5f), WHITE);
            }
        }
        
        public GameObjectHandler? GetGameObjectHandler()
        {
            return null;
        }
        public void Activate(IScene oldScene)
        {
            
            GAMELOOP.SwitchCursor(new SimpleCursorUI());
        }

        public void Deactivate()
        {
            GAMELOOP.SwitchCursor(new SimpleCursorGameUI());
        }

        public void Close()
        {
            
        }

        

        public void DrawGame(ScreenInfo game)
        {
            
        }
    }

}
