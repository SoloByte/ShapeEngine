namespace ShapeEngine.Input;

public class ShapeGamepadButtonInput : IShapeInputType
{
    private readonly ShapeGamepadButton button;
    private ShapeInputState state = new();
    private readonly float deadzone;

    public ShapeGamepadButtonInput(ShapeGamepadButton button, float deadzone = 0.2f)
    {
        this.button = button; 
        this.deadzone = deadzone;
    }
    
    public IShapeInputType Copy() => new ShapeGamepadButtonInput(button, deadzone);
    public string GetName(bool shorthand = true) => GetGamepadButtonName(button, shorthand);
    public void Update(float dt, int gamepadIndex)
    {
        state = GetState(button, state, gamepadIndex, deadzone);
    }
    public ShapeInputState GetState() => state;
    
    private static bool IsDown(ShapeGamepadButton button, int gamepadIndex, float deadzone = 0.2f)
    {
        var id = (int)button;
        if (id >= 30 && id <= 33)
        {
            id -= 30;
            float value = GetGamepadAxisMovement(gamepadIndex, id);
            if (MathF.Abs(value) < deadzone) value = 0f;
            return value > 0f;
        }
        
        if (id >= 40 && id <= 43)
        {
            id -= 40;
            float value = GetGamepadAxisMovement(gamepadIndex, id);
            if (MathF.Abs(value) < deadzone) value = 0f;
            return value < 0f;
        }
        
        return IsGamepadButtonDown(gamepadIndex, id);
    }
    public static ShapeInputState GetState(ShapeGamepadButton button, int gamepadIndex, float deadzone = 0.2f)
    {
        bool down = IsDown(button, gamepadIndex, deadzone);
        return new(down, !down, 0f, gamepadIndex);
    }

    public static ShapeInputState GetState(ShapeGamepadButton button, ShapeInputState previousState, int gamepadIndex,
        float deadzone = 0.2f)
    {
        return new(previousState, GetState(button, gamepadIndex, deadzone));
    }
    public static string GetGamepadButtonName(ShapeGamepadButton button, bool shortHand = true)
    {
        switch (button)
        {
            case ShapeGamepadButton.UNKNOWN: return shortHand ? "Unknown" : "GP Button Unknown";
            case ShapeGamepadButton.LEFT_FACE_UP: return shortHand ? "Up" : "GP Button Up";
            case ShapeGamepadButton.LEFT_FACE_RIGHT: return shortHand ? "Right" : "GP Button Right";
            case ShapeGamepadButton.LEFT_FACE_DOWN: return shortHand ? "Down" : "GP Button Down";
            case ShapeGamepadButton.LEFT_FACE_LEFT: return shortHand ? "Left" : "GP Button Left";
            case ShapeGamepadButton.RIGHT_FACE_UP: return shortHand ? "Y" : "GP Button Y";
            case ShapeGamepadButton.RIGHT_FACE_RIGHT: return shortHand ? "B" : "GP Button B";
            case ShapeGamepadButton.RIGHT_FACE_DOWN: return shortHand ? "A" : "GP Button A";
            case ShapeGamepadButton.RIGHT_FACE_LEFT: return shortHand ? "X" : "GP Button X";
            case ShapeGamepadButton.LEFT_TRIGGER_TOP: return shortHand ? "LB" : "GP Button Left Bumper";
            case ShapeGamepadButton.LEFT_TRIGGER_BOTTOM: return shortHand ? "LT" : "GP Button Left Trigger";
            case ShapeGamepadButton.RIGHT_TRIGGER_TOP: return shortHand ? "RB" : "GP Button Right Bumper";
            case ShapeGamepadButton.RIGHT_TRIGGER_BOTTOM: return shortHand ? "RT" : "GP Button Right Trigger";
            case ShapeGamepadButton.MIDDLE_LEFT: return shortHand ? "Select" : "GP Button Select";
            case ShapeGamepadButton.MIDDLE: return shortHand ? "Home" : "GP Button Home";
            case ShapeGamepadButton.MIDDLE_RIGHT: return shortHand ? "Start" : "GP Button Start";
            case ShapeGamepadButton.LEFT_THUMB: return shortHand ? "LClick" : "GP Button Left Stick Click";
            case ShapeGamepadButton.RIGHT_THUMB: return shortHand ? "RClick" : "GP Button Right Stick Click";
            case ShapeGamepadButton.LEFT_STICK_RIGHT: return shortHand ? "LS R" : "Left Stick Right";
            case ShapeGamepadButton.LEFT_STICK_LEFT: return shortHand ? "LS L" : "Left Stick Left";
            case ShapeGamepadButton.LEFT_STICK_DOWN: return shortHand ? "LS D" : "Left Stick Down";
            case ShapeGamepadButton.LEFT_STICK_UP: return shortHand ? "LS U" : "Left Stick Up";
            case ShapeGamepadButton.RIGHT_STICK_RIGHT: return shortHand ? "RS R" : "Right Stick Right";
            case ShapeGamepadButton.RIGHT_STICK_LEFT: return shortHand ? "RS L" : "Right Stick Left";
            case ShapeGamepadButton.RIGHT_STICK_DOWN: return shortHand ? "RS D" : "Right Stick Down";
            case ShapeGamepadButton.RIGHT_STICK_UP: return shortHand ? "RS U" : "Right Stick Up";
            default: return "No Key";
        }
    }

}