namespace ShapeEngine.Input;

public class InputTypeMouseButton : IInputType
{
    private readonly ShapeMouseButton button;
    public InputTypeMouseButton(ShapeMouseButton button) { this.button = button; }
    public float GetDeadzone() => 0f;

    public void SetDeadzone(float value) { }
    public InputState GetState(int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(button);
    }

    public InputState GetState(InputState prev, int gamepad = -1)
    {
        if (gamepad > 0) return new();
        return GetState(button, prev);
    }
    public virtual string GetName(bool shorthand = true) => GetMouseButtonName(button, shorthand);
    public InputDevice GetInputDevice() => InputDevice.Mouse;

    public IInputType Copy() => new InputTypeMouseButton(button);
        
        
    public static string GetMouseButtonName(ShapeMouseButton button, bool shortHand = true)
    {
        switch (button)
        {
            case ShapeMouseButton.LEFT: return shortHand ? "LMB" : "Left Mouse Button";
            case ShapeMouseButton.RIGHT: return shortHand ? "RMB" : "Right Mouse Button";
            case ShapeMouseButton.MIDDLE: return shortHand ? "MMB" : "Middle Mouse Button";
            case ShapeMouseButton.SIDE: return shortHand ? "SMB" : "Side Mouse Button";
            case ShapeMouseButton.EXTRA: return shortHand ? "EMB" : "Extra Mouse Button";
            case ShapeMouseButton.FORWARD: return shortHand ? "FMB" : "Forward Mouse Button";
            case ShapeMouseButton.BACK: return shortHand ? "BMB" : "Back Mouse Button";
            case ShapeMouseButton.MW_UP: return shortHand ? "MW U" : "Mouse Wheel Up";
            case ShapeMouseButton.MW_DOWN: return shortHand ? "MW D" : "Mouse Wheel Down";
            case ShapeMouseButton.MW_LEFT: return shortHand ? "MW L" : "Mouse Wheel Left";
            case ShapeMouseButton.MW_RIGHT: return shortHand ? "MW R" : "Mouse Wheel Right";
            case ShapeMouseButton.UP_AXIS: return shortHand ? "M Up" : "Mouse Up";
            case ShapeMouseButton.DOWN_AXIS: return shortHand ? "M Dwn" : "Mouse Down";
            case ShapeMouseButton.LEFT_AXIS: return shortHand ? "M Lft" : "Mouse Left";
            case ShapeMouseButton.RIGHT_AXIS: return shortHand ? "M Rgt" : "Mouse Right";
            default: return "No Key";
        }
    }
    private static bool IsDown(ShapeMouseButton button)
    {
        var id = (int)button;
        if (id >= 10)
        {
            var value = GetMouseWheelMoveV();
            return button switch
            {
                ShapeMouseButton.MW_LEFT => value.X < 0f,
                ShapeMouseButton.MW_RIGHT => value.X > 0f,
                ShapeMouseButton.MW_UP => value.Y < 0f,
                ShapeMouseButton.MW_DOWN => value.Y > 0f,
                _ => false
            };
        }
            
        return IsMouseButtonDown(id);
    }
    public static InputState GetState(ShapeMouseButton button)
    {
        bool down = IsDown(button);
        return new(down, !down, down ? 1f : 0f, -1);
    }
    public static InputState GetState(ShapeMouseButton button, InputState previousState)
    {
        return new(previousState, GetState(button));
    }
}