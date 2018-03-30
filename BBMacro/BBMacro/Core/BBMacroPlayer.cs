using System.Collections.Generic;

public class BBMacroPlayer
{
    private bool mDisposed;

    private BBConfig mConfig;

    private IBBInputModuleProxy mModuleProxy;

    private BBInputEmulator mEmulator;

    private bool mIsPlaying;

    private bool mIsPaused;

    private BBMacro mHead;

    private BBMacro mCurrent;

    private int mCurrentTimes;

    private float mCurrentDelay = 0;

    private float mCurrentTime = 0;

    private List<BBMacroPlayer> mInterrupts;

    private Queue<BBMacroPlayer> mSeriesQueue;

    public BBMacroPlayer(BBConfig config, IBBInputModuleProxy moduleProxy)
    {
        mConfig = config;
        mModuleProxy = moduleProxy;
        mEmulator = new BBInputEmulator(mModuleProxy);
        mIsPlaying = false;
        mIsPaused = false;
        mInterrupts = new List<BBMacroPlayer>();
        mSeriesQueue = new Queue<BBMacroPlayer>();
    }

    public void Set(BBMacro macro)
    {
        mHead = macro;
    }

    public void Play()
    {
        if (!mIsPlaying)
        {
            mIsPlaying = true;
            mCurrent = mHead;
            mCurrent.RunScript();
            mCurrentTimes = 1;
            mCurrentTime = 0;
            mCurrentDelay = mCurrent.delay;
            mEmulator.Clear();
        }
    }

    public void Stop()
    {
        if (mIsPlaying)
        {
            mIsPlaying = false;
        }
    }

    public void Pause(bool paused)
    {
        mIsPaused = paused;
    }

    public void Update(float deltaTime)
    {
        if (mIsPlaying && !mIsPaused)
        {
            if (mInterrupts.Count > 0)
            {
                for (int i = mInterrupts.Count - 1; i >= 0; i--)
                {
                    if (!mInterrupts[i].isPlaying)
                    {
                        mInterrupts[i].Dispose();
                        mInterrupts.RemoveAt(i);
                    }
                    else
                    {
                        mInterrupts[i].Update(deltaTime);
                    }
                }
            }
            else if (mSeriesQueue.Count > 0)
            {
                BBMacroPlayer player = mSeriesQueue.Dequeue();
                player.Play();
                mInterrupts.Add(player);
            }
            else if (mCurrent == null)
            {
                mIsPlaying = false;
            }
            else
            {
                if (mCurrentTime > 0)
                {
                    mCurrentTime -= deltaTime;
                }
                else
                {
                    mCurrentDelay -= deltaTime;
                }
                if (mEmulator.isPlaying)
                {
                    mEmulator.Update(deltaTime);
                }
                else
                {
                    while ((mCurrentDelay < 0 || mCurrentTime < 0) && 
                        !mEmulator.isPlaying && mInterrupts.Count == 0)
                    {
                        if (mCurrentTime < 0)
                        {
                            if (mCurrentTimes < mCurrent.times)
                            {
                                mCurrentTimes += 1;
                            }
                            else
                            {
                                mCurrent = mCurrent.next;
                                if (mCurrent == null)
                                {
                                    break;
                                }
                            }
                            mCurrent.RunScript();
                            mCurrentDelay += mCurrent.delay;
                            mCurrentDelay += mCurrentTime;
                            mCurrentTime = 0;
                        }
                        if (mCurrentDelay < 0)
                        {
                            BBLoopAction action = BBLoopAction.Continue;
                            Execute(mCurrent, out action);
                            if (action == BBLoopAction.Continue)
                            {
                                mCurrentTime += mCurrent.duration;
                                mCurrentTime += mCurrentDelay;
                            }
                            else if (action == BBLoopAction.Break)
                            {
                                mCurrentTimes = mCurrent.times;
                                mCurrentTime = -float.Epsilon;
                            }
                            mCurrentDelay = 0;
                        }
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        mConfig = null;
        mEmulator = null;
        mHead = null;
        mCurrent = null;
        if (mInterrupts != null)
        {
            for (int i = mInterrupts.Count - 1; i >= 0; i--)
            {
                mInterrupts[i].Dispose();
            }
            mInterrupts = null;
        }
        mDisposed = true;
    }

    private void Execute(BBMacro macro, out BBLoopAction action)
    {
        action = BBLoopAction.Continue;
        switch (macro.macroType)
        {
            case BBMacroType.If:
                BBIfMacro ifmacro = macro as BBIfMacro;
                BBMacro future = null;
                if (ifmacro.Test(out future))
                {
                    BBMacroPlayer inserted = new BBMacroPlayer(mConfig, mModuleProxy);
                    inserted.Set(future);
                    inserted.Play();
                    mInterrupts.Add(inserted);
                }
                else
                {
                    action = ifmacro.action;
                }
                break;
            case BBMacroType.Series:
                BBSeriesMacro srmacro = macro as BBSeriesMacro;
                foreach (BBMacro item in srmacro.items)
                {
                    BBMacroPlayer inserted = new BBMacroPlayer(mConfig, mModuleProxy);
                    inserted.Set(item);
                    mSeriesQueue.Enqueue(inserted);
                }
                break;
            case BBMacroType.Parallel:
                BBParallelMacro prmacro = macro as BBParallelMacro;
                foreach (BBMacro item in prmacro.items)
                {
                    BBMacroPlayer inserted = new BBMacroPlayer(mConfig, mModuleProxy);
                    inserted.Set(item);
                    inserted.Play();
                    mInterrupts.Add(inserted);
                }
                break;
            default:
                mEmulator.Set(Expand(macro));
                break;
        }
    }

    private BBInputSnapshot Expand(BBMacro macro)
    {
        BBInputSnapshot expand = new BBInputSnapshot();
        expand.button = macro.button;
        switch (macro.code)
        {
            case BBMacroOpCode.Press:
                {
                    expand.type = BBInputType.Press;
                    expand.inputPosition = mConfig.Axis2Screen(new float[] { macro.data[0], macro.data[1] });
                    expand.duration = macro.duration;
                    break;
                }
            case BBMacroOpCode.Release:
                {
                    expand.type = BBInputType.Release;
                    expand.inputPosition = mConfig.Axis2Screen(new float[] { macro.data[0], macro.data[1] });
                    expand.duration = macro.duration;
                    break;
                }
            case BBMacroOpCode.Move:
                {
                    expand.type = BBInputType.Move;
                    expand.inputPosition = mConfig.Axis2Screen(new float[] { macro.data[0], macro.data[1] });
                    expand.duration = macro.duration;
                    break;
                }
            case BBMacroOpCode.Click:
                {
                    float[] inputPosition = mConfig.Axis2Screen(new float[] { macro.data[0], macro.data[1] });
                    expand.type = BBInputType.Press;
                    expand.inputPosition = inputPosition;
                    expand.duration = macro.duration;
                    expand.next = new BBInputSnapshot()
                    {
                        button = expand.button,
                        type = BBInputType.Release,
                        inputPosition = inputPosition,
                    };
                    break;
                }
            case BBMacroOpCode.Drag:
                {
                    expand.type = BBInputType.Press;
                    expand.inputPosition = mConfig.Axis2Screen(new float[] { macro.data[0], macro.data[1] });
                    int dataLen2 = macro.data.Length / 2;
                    float duration = macro.duration / dataLen2;
                    BBInputSnapshot current = expand;
                    for (int i = 0; i < dataLen2; i++)
                    {
                        current.next = new BBInputSnapshot()
                        {
                            button = current.button,
                            type = BBInputType.Move,
                            inputPosition = mConfig.Axis2Screen(new float[] { macro.data[i * 2], macro.data[i * 2 + 1] }),
                            duration = duration,
                        };
                        current = current.next;
                    }
                    current.next = new BBInputSnapshot()
                    {
                        button = current.button,
                        type = BBInputType.Release,
                        inputPosition = current.inputPosition,
                    };
                    break;
                }
            case BBMacroOpCode.Wheel:
                {
                    expand.type = BBInputType.Wheel;
                    expand.inputPosition = mConfig.Axis2Screen(new float[] { macro.data[0], macro.data[1] });
                    expand.delta = (int)macro.data[2];
                    expand.duration = macro.duration;
                    break;
                }
            case BBMacroOpCode.KeyDown:
                {
                    expand.type = BBInputType.KeyDown;
                    expand.key = macro.key;
                    expand.duration = macro.duration;
                    break;
                }
            case BBMacroOpCode.KeyUp:
                {
                    expand.type = BBInputType.KeyUp;
                    expand.key = macro.key;
                    expand.duration = macro.duration;
                    break;
                }
            case BBMacroOpCode.KeyPress:
                {
                    expand.type = BBInputType.KeyDown;
                    expand.key = macro.key;
                    expand.duration = macro.duration;
                    expand.next = new BBInputSnapshot()
                    {
                        type = BBInputType.KeyUp,
                        key = macro.key
                    };
                    break;
                }
        }
        return expand;
    }

    public bool disposed
    {
        get
        {
            return mDisposed;
        }
    }

    public bool isPlaying
    {
        get
        {
            return mIsPlaying;
        }
    }

    public bool isPaused
    {
        get
        {
            return mIsPaused;
        }
    }

    public BBMacro head
    {
        get
        {
            return mHead;
        }
    }

    public BBMacro current
    {
        get
        {
            return mCurrent;
        }
    }

    public int currentTimes
    {
        get
        {
            return mCurrentTimes;
        }
    }

    public float currentTime
    {
        get
        {
            return mCurrentTime;
        }
    }
}