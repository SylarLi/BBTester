public class BBInputSnapshot
{
    public BBInputType type;

    public BBInputButton button;

    public BBVirtualKey key;

    public float[] inputPosition;

    public int delta;

    // ---------------- state for emulator ------------------ //

    public float timeStamp = 0;

    public float delay = 0;

    public float duration = 0;

    public BBInputSnapshot next;

    // ---------------- state for module proxy -------------- //

    public bool isPressed = false;
}