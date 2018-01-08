using System.Collections.Generic;

/// <summary>
/// 捕获一次操作，以Release或KeyUp为中止点
/// </summary>
public class BBMacroCapture
{
    private bool mDisposed;

    private BBConfig mConfig;

    private IBBInputModuleProxy mModuleProxy;

    private BBInputEmulator mEmulator;

    private BBMacro mCaptureResult;

    public BBMacroCapture(BBConfig config, IBBInputModuleProxy moduleProxy)
    {
        mConfig = config;
        mModuleProxy = moduleProxy;
        mEmulator = new BBInputEmulator(mModuleProxy);
    }

    public void Update(float deltaTime)
    {
        mEmulator.Update(deltaTime);
        BBInputSnapshot current = mEmulator.recordHead;
        while (current != null)
        {
            if (current.type == BBInputType.Release || 
                current.type == BBInputType.KeyUp)
            {
                mCaptureResult = StopCapture();
                break;
            }
            current = current.next;
        }
    }

    public void StartCapture()
    {
        mEmulator.StartRecord(mConfig.monitoringButtons, mConfig.monitoringKeyboard);
    }

    private BBMacro StopCapture()
    {
        if (!isCapturing)
        {
            throw new System.InvalidOperationException();
        }
        mEmulator.StopRecord();
        BBMacro head = null;
        BBMacro current = null;
        BBInputSnapshot snapTail = null;
        bool parted = false;
        var elements = new List<BBInputSnapshot>();
        var currentSnap = mEmulator.recordHead;
        while (currentSnap != null)
        {
            parted = false;
            elements.Add(currentSnap);
            if (elements.Count == 1)
            {
                if (currentSnap.type == BBInputType.Press ||
                    currentSnap.type == BBInputType.KeyDown)
                {
                    // Wait to merge
                }
                else
                {
                    parted = true;
                }
            }
            if (elements.Count > 1)
            {
                if (currentSnap.type == BBInputType.Move)
                {
                    // Wait to merge
                }
                else if ((currentSnap.type == BBInputType.Release &&
                    currentSnap.button == elements[0].button) ||
                    (currentSnap.type == BBInputType.KeyUp &&
                    currentSnap.key == elements[0].key))
                {
                    var fusion = Merge(elements);
                    if (head == null)
                    {
                        head = fusion;
                        current = head;
                    }
                    else
                    {
                        current.next = fusion;
                        current = current.next;
                        current.delay = elements[0].timeStamp - snapTail.timeStamp;
                    }
                    snapTail = currentSnap;
                    elements.Clear();
                }
                else
                {
                    parted = true;
                }
            }
            if (parted)
            {
                BBMacro fragment = Parse(elements);
                if (head == null)
                {
                    head = fragment;
                    current = head;
                }
                else
                {
                    current.next = fragment;
                    current = current.next;
                }
                for (int i = 0; i < elements.Count; i++)
                {
                    if (snapTail != null)
                    {
                        current.delay = elements[i].timeStamp - snapTail.timeStamp;
                    }
                    snapTail = elements[i];
                    if (i != elements.Count - 1)
                    {
                        current = current.next;
                    }
                }
                elements.Clear();
            }
            if (currentSnap.type == BBInputType.Release ||
                currentSnap.type == BBInputType.KeyUp)
            {
                break;
            }
            currentSnap = currentSnap.next;
        }
        return head;
    }

    public void Dispose()
    {
        mConfig = null;
        mEmulator = null;
        mCaptureResult = null;
        mDisposed = true;
    }

    private BBMacro Merge(List<BBInputSnapshot> snapshots)
    {
        if (snapshots.Count < 2)
        {
            throw new System.InvalidOperationException();
        }
        if (snapshots[0].type == BBInputType.Press)
        {
            if (snapshots[snapshots.Count - 1].type != BBInputType.Release)
            {
                throw new System.InvalidOperationException();
            }
            BBMacro macro = new BBMacro();
            macro.button = snapshots[0].button;
            macro.duration = snapshots[snapshots.Count - 1].timeStamp - snapshots[0].timeStamp;
            if (snapshots.Count == 2)
            {
                macro.code = BBMacroOpCode.Click;
                var coord = mConfig.Screen2Axis(snapshots[0].inputPosition);
                macro.data = coord;
            }
            else
            {
                macro.code = BBMacroOpCode.Drag;
                macro.data = new float[snapshots.Count * 2];
                for (int i = 0; i < snapshots.Count; i++)
                {
                    var coord = mConfig.Screen2Axis(snapshots[i].inputPosition);
                    macro.data[i * 2] = coord[0];
                    macro.data[i * 2 + 1] = coord[1];
                }
            }
            return macro;
        }
        else if (snapshots[0].type == BBInputType.KeyDown)
        {
            if (snapshots.Count != 2 ||
                snapshots[1].type != BBInputType.KeyUp)
            {
                throw new System.InvalidOperationException();
            }
            BBMacro macro = new BBMacro();
            macro.code = BBMacroOpCode.KeyPress;
            macro.key = snapshots[0].key;
            macro.duration = snapshots[1].timeStamp - snapshots[0].timeStamp;
            return macro;
        }
        else
        {
            throw new System.InvalidOperationException();
        }
    }

    private BBMacro Parse(List<BBInputSnapshot> snapshots)
    {
        BBMacro head = null;
        BBMacro current = null;
        for (int i = 0; i < snapshots.Count; i++)
        {
            BBInputSnapshot snapshot = snapshots[i];
            BBMacro macro = new BBMacro();
            macro.button = snapshot.button;
            macro.code = BBInputUtil.InputType2OpCode(snapshot.type);
            macro.key = snapshot.key;
            if (BBInputUtil.IsMouseInputType(snapshot.type))
            {
                var coord = mConfig.Screen2Axis(snapshot.inputPosition);
                if (snapshot.type == BBInputType.Wheel)
                {
                    macro.data = new float[] { coord[0], coord[1], snapshot.delta };
                }
                else
                {
                    macro.data = coord;
                }
            }
            if (head == null)
            {
                head = macro;
                current = head;
            }
            else
            {
                current.next = macro;
                current = current.next;
            }
        }
        return head;
    }

    public bool disposed
    {
        get
        {
            return mDisposed;
        }
    }

    public bool isCapturing
    {
        get
        {
            return mEmulator.isRecording;
        }
    }

    public BBMacro captureResult
    {
        get
        {
            return mCaptureResult;
        }
    }
}