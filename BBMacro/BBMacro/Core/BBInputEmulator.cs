public class BBInputEmulator
{
    private IBBInputModuleProxy mInputProxy;

    private BBInputSnapshot mHead;

    private BBInputSnapshot mCurrent;

    private float mCurrentDelay = 0;

    private float mCurrentTime = 0;

    private bool mIsRecording;

    private BBInputButton mRecordButtons;

    private bool mRecordKeyboard;

    private BBInputSnapshot mRecordHead;

    private BBInputSnapshot mRecordCurrent;

    private float mRecordTime = 0;

    public BBInputEmulator(IBBInputModuleProxy inputProxy)
    {
        mInputProxy = inputProxy;
    }

    public void Set(BBInputSnapshot snapshot)
    {
        mHead = snapshot;
        mCurrent = mHead;
        mCurrentTime = 0;
        mCurrentDelay = mCurrent.delay;
    }

    public void Update(float deltaTime)
    {
        if (mCurrent != null)
        {
            if (mCurrentTime > 0)
            {
                mCurrentTime -= deltaTime;
            }
            else
            {
                mCurrentDelay -= deltaTime;
            }
            while (mCurrentDelay < 0 || mCurrentTime < 0)
            {
                if (mCurrentTime < 0)
                {
                    mCurrent = mCurrent.next;
                    if (mCurrent != null)
                    {
                        mCurrentDelay += mCurrent.delay;
                        mCurrentDelay += mCurrentTime;
                        mCurrentTime = 0;
                    }
                    else
                    {
                        break;
                    }
                }
                if (mCurrentDelay < 0)
                {
                    Execute(mCurrent);
                    mCurrentTime += mCurrent.duration;
                    mCurrentTime += mCurrentDelay;
                    mCurrentDelay = 0;
                }
            }
        }
        if (mIsRecording)
        {
            mRecordTime += deltaTime;
            BBInputSnapshot snapshot = mInputProxy.TakeSnapshot();
            while (snapshot != null)
            {
                if (IsNiceSnapshot(snapshot))
                {
                    snapshot.timeStamp = mRecordTime;
                    if (mRecordHead == null)
                    {
                        mRecordHead = snapshot;
                        mRecordCurrent = mRecordHead;
                    }
                    else
                    {
                        mRecordCurrent.next = snapshot;
                        mRecordCurrent = snapshot;
                    }
                }
                snapshot = mInputProxy.TakeSnapshot();
            }
        }
    }

    public void Clear()
    {
        mCurrent = null;
        mRecordHead = null;
        mRecordCurrent = null;
    }

    public void Execute(BBInputSnapshot snapshot)
    {
        mInputProxy.ProcessInput(snapshot);
    }

    public void StartRecord(BBInputButton buttons, bool keyboard)
    {
        mIsRecording = true;
        mRecordButtons = buttons;
        mRecordKeyboard = keyboard;
        mRecordTime = 0;
        mRecordHead = null;
        mRecordCurrent = null;
        mInputProxy.OnRecordStart();
    }

    public void StopRecord()
    {
        mIsRecording = false;
        mInputProxy.OnRecordStop();
    }

    private bool IsNiceSnapshot(BBInputSnapshot snapshot)
    {
        bool ret = false;
        if (BBUtil.IsStrokeInputType(snapshot.type))
        {
            if (snapshot.type == BBInputType.Move ||
                snapshot.type == BBInputType.Wheel)
            {
                if (mRecordButtons > 0)
                {
                    ret = true;
                }
            }
            else if ((mRecordButtons & snapshot.button) > 0)
            {
                ret = true;
            }
        }
        else if (BBUtil.IsKeyboardInputType(snapshot.type) && mRecordKeyboard)
        {
            ret = true;
        }
        return ret;
    }

    public bool isPlaying
    {
        get
        {
            return mCurrent != null;
        }
    }

    public BBInputSnapshot current
    {
        get
        {
            return mCurrent;
        }
    }

    public float currenDelay
    {
        get
        {
            return mCurrentDelay;
        }
    }

    public float currentTime
    {
        get
        {
            return mCurrentTime;
        }
    }

    public bool isRecording
    {
        get
        {
            return mIsRecording;
        }
    }

    public BBInputButton recordButtons
    {
        get
        {
            return mRecordButtons;
        }
    }

    public bool recordKeyboard
    {
        get
        {
            return mRecordKeyboard;
        }
    }

    public BBInputSnapshot recordHead
    {
        get
        {
            return mRecordHead;
        }
    }

    public BBInputSnapshot recordCurrent
    {
        get
        {
            return mRecordCurrent;
        }
    }
}