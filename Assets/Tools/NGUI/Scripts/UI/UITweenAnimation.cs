using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UITweenAnimation : MonoBehaviour 
{
    //tweenposition animation
    public TweenPosition[] EnterPosAnimation = null;
    public TweenPosition[] ExitPosAnimation = null;
    private Vector3[] enterPosFrom = null;
    private Vector3[] enterPosTo = null;
    private Vector3[] exitPosFrom = null;
    private Vector3[] exitPosTo = null;
    private float[] enterPosDuration = null;
    private float[] exitPosDuration = null;
    private AnimationCurve[] enterPosCurve = null;
    private AnimationCurve[] exitPosCurve = null;

    //TweenScale animation
    public TweenScale[] EnterScaleAnimation = null;
    public TweenScale[] ExitScaleAnimation = null;
    private Vector3[] enterScaleFrom = null;
    private Vector3[] enterScaleTo = null;
    private Vector3[] exitScaleFrom = null;
    private Vector3[] exitScaleTo = null;
    private float[] enterScaleDuration = null;
    private float[] exitScaleDuration = null;
    private AnimationCurve[] enterScaleCurve = null;
    private AnimationCurve[] exitScaleCurve = null;

    //TweenAlpha animation
    public TweenAlpha[] EnterAlphaAnimation = null;
    public TweenAlpha[] ExitAlphaAnimation = null;
    private float[] enterAlphaFrom = null;
    private float[] enterAlphaTo = null;
    private float[] exitAlphaFrom = null;
    private float[] exitAlphaTo = null;
    private float[] enterAlphaDuration = null;
    private float[] exitAlphaDuration = null;
    private AnimationCurve[] enterAlphaCurve = null;
    private AnimationCurve[] exitAlphaCurve = null;

    public TweenRotation[] EnterRotAnimation = null;
    public TweenRotation[] ExitRotAnimation = null;
    private Vector3[] enterRotFrom = null;
    private Vector3[] enterRotTo = null;
    private Vector3[] exitRotFrom = null;
    private Vector3[] exitRotTo = null;
    private float[] enterRotDuration = null;
    private float[] exitRotDuration = null;
    private AnimationCurve[] enterRotCurve = null;
    private AnimationCurve[] exitRotCurve = null;


    public float Duration = 0;

    public GameObject enterMaxDurationGo = null;
    public GameObject exitMaxDurationGo = null;

    public delegate void Notify();
    Notify finishAniCallBack = null;

    public AnimType type = AnimType.PopBig;

    [Tooltip("自定义弹出动效，只有在 type为 PopNone 的时候 生效")]
    public UITweener[] popTweeners; 

    public static List<UITweenAnimation> exitingAnimations = new List<UITweenAnimation>(); 

    public enum AnimType
    {
        PopNone=0,
        PopBig=1,
        PopMid=2,
        PopSmall=3,
    }

    void Awake()
    {
        if (EnterPosAnimation != null && EnterPosAnimation.Length > 0)
        {
            enterPosFrom = new Vector3[EnterPosAnimation.Length];
            enterPosTo = new Vector3[EnterPosAnimation.Length];
            enterPosDuration = new float[EnterPosAnimation.Length];
			enterPosCurve = new AnimationCurve[EnterPosAnimation.Length];

            for (int i = 0; i < EnterPosAnimation.Length; ++i)
            {
                if (EnterPosAnimation[i] != null)
                {
                    enterPosFrom[i] = EnterPosAnimation[i].from;
                    enterPosTo[i] = EnterPosAnimation[i].to;
                    enterPosDuration[i] = EnterPosAnimation[i].duration;
                    enterPosCurve[i] = EnterPosAnimation[i].animationCurve;
                    EnterPosAnimation[i].enabled = false;
                }
            }
        }

        if (EnterScaleAnimation != null && EnterScaleAnimation.Length > 0)
        {
            enterScaleFrom = new Vector3[EnterScaleAnimation.Length];
            enterScaleTo = new Vector3[EnterScaleAnimation.Length];
            enterScaleDuration = new float[EnterScaleAnimation.Length];
            enterScaleCurve = new AnimationCurve[EnterScaleAnimation.Length];
            for (int i = 0; i < EnterScaleAnimation.Length; ++i)
            {
                if (EnterScaleAnimation[i] != null)
                {
                    enterScaleFrom[i] = EnterScaleAnimation[i].from;
                    enterScaleTo[i] = EnterScaleAnimation[i].to;
                    enterScaleDuration[i] = EnterScaleAnimation[i].duration;
                    enterScaleCurve[i] = EnterScaleAnimation[i].animationCurve;
                    EnterScaleAnimation[i].enabled = false;
                }
            }
        }

        if (EnterAlphaAnimation != null && EnterAlphaAnimation.Length > 0)
        {
            enterAlphaFrom = new float[EnterAlphaAnimation.Length];
            enterAlphaTo = new float[EnterAlphaAnimation.Length];
            enterAlphaDuration = new float[EnterAlphaAnimation.Length];
            enterAlphaCurve = new AnimationCurve[EnterAlphaAnimation.Length];
            for (int i = 0; i < EnterAlphaAnimation.Length; ++i)
            {
                if (EnterAlphaAnimation[i] != null)
                {
                    enterAlphaFrom[i] = EnterAlphaAnimation[i].from;
                    enterAlphaTo[i] = EnterAlphaAnimation[i].to;
                    enterAlphaDuration[i] = EnterAlphaAnimation[i].duration;
                    enterAlphaCurve[i] = EnterAlphaAnimation[i].animationCurve;
                    EnterAlphaAnimation[i].enabled = false;
                }
            }
        }

        if (EnterRotAnimation != null && EnterRotAnimation.Length > 0)
        {
            enterRotFrom = new Vector3[EnterRotAnimation.Length];
            enterRotTo = new Vector3[EnterRotAnimation.Length];
            enterRotDuration = new float[EnterRotAnimation.Length];
            enterRotCurve = new AnimationCurve[EnterRotAnimation.Length];
            for (int i = 0; i < EnterRotAnimation.Length; ++i)
            {
                if (EnterRotAnimation[i] != null)
                {
                    enterRotFrom[i] = EnterRotAnimation[i].from;
                    enterRotTo[i] = EnterRotAnimation[i].to;
                    enterRotDuration[i] = EnterRotAnimation[i].duration;
                    enterRotCurve[i] = EnterRotAnimation[i].animationCurve;
                    EnterRotAnimation[i].enabled = false;
                }
            }
        }


        if (ExitPosAnimation != null && ExitPosAnimation.Length > 0)
        {
            exitPosFrom = new Vector3[ExitPosAnimation.Length];
            exitPosTo = new Vector3[ExitPosAnimation.Length];
            exitPosDuration = new float[ExitPosAnimation.Length];
			exitPosCurve = new AnimationCurve[ExitPosAnimation.Length];
            for (int i = 0; i < ExitPosAnimation.Length; ++i)
            {
                if (ExitPosAnimation[i] != null)
                {
                    exitPosFrom[i] = ExitPosAnimation[i].from;
                    exitPosTo[i] = ExitPosAnimation[i].to;
                    exitPosDuration[i] = ExitPosAnimation[i].duration;
                    exitPosCurve[i] = ExitPosAnimation[i].animationCurve;
                    ExitPosAnimation[i].enabled = false;
                }
            }
        }

        if (ExitScaleAnimation != null && ExitScaleAnimation.Length > 0)
        {
            exitScaleFrom = new Vector3[ExitScaleAnimation.Length];
            exitScaleTo = new Vector3[ExitScaleAnimation.Length];
            exitScaleDuration = new float[ExitScaleAnimation.Length];
			exitScaleCurve = new AnimationCurve[ExitScaleAnimation.Length];
            for (int i = 0; i < ExitScaleAnimation.Length; ++i)
            {
                if (ExitScaleAnimation[i] != null)
                {
                    exitScaleFrom[i] = ExitScaleAnimation[i].from;
                    exitScaleTo[i] = ExitScaleAnimation[i].to;
                    exitScaleDuration[i] = ExitScaleAnimation[i].duration;
                    exitScaleCurve[i] = ExitScaleAnimation[i].animationCurve;
                    ExitScaleAnimation[i].enabled = false;
                }
            }
        }
    
        if (ExitAlphaAnimation != null && ExitAlphaAnimation.Length > 0)
        {
			exitAlphaFrom = new float[ExitAlphaAnimation.Length];
			exitAlphaTo = new float[ExitAlphaAnimation.Length];
			exitAlphaDuration = new float[ExitAlphaAnimation.Length];
            exitAlphaCurve = new AnimationCurve[ExitAlphaAnimation.Length];
            for (int i = 0; i < ExitAlphaAnimation.Length; ++i)
            {
                if (ExitAlphaAnimation[i] != null)
                {
                    exitAlphaFrom[i] = ExitAlphaAnimation[i].from;
                    exitAlphaTo[i] = ExitAlphaAnimation[i].to;
                    exitAlphaDuration[i] = ExitAlphaAnimation[i].duration;
                    exitAlphaCurve[i] = ExitAlphaAnimation[i].animationCurve;
                    ExitAlphaAnimation[i].enabled = false;
                }
            }
        }

        if (ExitRotAnimation != null && ExitRotAnimation.Length > 0)
        {
            exitRotFrom = new Vector3[ExitRotAnimation.Length];
            exitRotTo = new Vector3[ExitRotAnimation.Length];
            exitRotDuration = new float[ExitRotAnimation.Length];
            exitRotCurve = new AnimationCurve[ExitRotAnimation.Length];
            for (int i = 0; i < ExitRotAnimation.Length; ++i)
            {
                if (ExitRotAnimation[i] != null)
                {
                    exitRotFrom[i] = ExitRotAnimation[i].from;
                    exitRotTo[i] = ExitRotAnimation[i].to;
                    exitRotDuration[i] = ExitRotAnimation[i].duration;
                    exitRotCurve[i] = ExitRotAnimation[i].animationCurve;
                    ExitRotAnimation[i].enabled = false;
                }
            }
        }
    }

    Queue<EventDelegate.Callback> readyPlaying = new Queue<EventDelegate.Callback>();
    Queue<UITweener> playingQueue = new Queue<UITweener>();

    /// <summary>
    /// 显示时的回调方法
    /// </summary>
    /// 
    public bool OnShow(bool isWaitForPlay = true, Notify _notify = null)
    {
        // ShowEnterFull();
        switch (type)
        {

            case AnimType.PopBig:
            case AnimType.PopMid:
            case AnimType.PopSmall:
                ShoEnterPopBig();
                break;
            case AnimType.PopNone:
                ShowSelfDefinePop();
                break;
        }
        if (playingQueue.Count > 0 && !isWaitForPlay) return false;
        finishAniCallBack = _notify;
        PlayEnterAnimation();
        return true;
    }

    private void ShowSelfDefinePop()
    {
        Transform gTransform = gameObject.transform;
        foreach (UITweener itemTweener in popTweeners)
        {
            if (itemTweener is TweenAlpha)
            {
                TweenAlpha itemAlpha = itemTweener as TweenAlpha; ;
                TweenAlpha tweenAlpha = gameObject.AddMissingComponent<TweenAlpha>();
                tweenAlpha.enabled = false;
                tweenAlpha.mTrans = gTransform;
                tweenAlpha = TweenAlpha.Begin(gameObject, itemAlpha.duration, itemAlpha.to);

                tweenAlpha.delay = itemAlpha.delay;
                tweenAlpha.duration = itemTweener.duration;
                tweenAlpha.animationCurve = itemTweener.animationCurve;
                tweenAlpha.from = itemAlpha.from;
                tweenAlpha.to = itemAlpha.to;

               
                tweenAlpha.onFinished.Clear();
                tweenAlpha.AddOnFinished(FinishEnterAnimation);
                playingQueue.Enqueue(tweenAlpha);
            }
            else if (itemTweener is TweenPosition)
            {
                TweenPosition itemPosition = itemTweener as TweenPosition;
                TweenPosition tweenPosition = gameObject.AddMissingComponent<TweenPosition>();
                tweenPosition.enabled = false;

                tweenPosition.mTrans = gTransform;
                tweenPosition = TweenPosition.Begin(gameObject, itemPosition.duration, itemPosition.to + gTransform.localPosition);

              
                tweenPosition.delay = itemPosition.delay;
                tweenPosition.duration = itemPosition.duration;
                tweenPosition.animationCurve = itemPosition.animationCurve;
                tweenPosition.from = itemPosition.from + gTransform.localPosition;
                tweenPosition.to = itemPosition.to + gTransform.localPosition;
                gTransform.localPosition = tweenPosition.from;

               
                tweenPosition.onFinished.Clear();
                tweenPosition.AddOnFinished(FinishEnterAnimation);
                playingQueue.Enqueue(tweenPosition);
            }
            else if (itemTweener is TweenScale)
            {
                TweenScale itemScale = itemTweener as TweenScale;
                TweenScale tweenScale = gameObject.AddMissingComponent<TweenScale>();
                tweenScale.enabled = false;
                tweenScale.mTrans = gTransform;
                tweenScale = TweenScale.Begin(gameObject, itemScale.duration, itemScale.to);

                tweenScale.delay = itemScale.delay;
                tweenScale.duration = itemScale.duration;
                tweenScale.animationCurve = itemScale.animationCurve;
                tweenScale.from = itemScale.from;
                tweenScale.to = itemScale.to;
                gTransform.localScale = tweenScale.from;


               // tweenScale.value = Vector3.zero;
               
                tweenScale.onFinished.Clear();
                tweenScale.AddOnFinished(FinishEnterAnimation);
                playingQueue.Enqueue(tweenScale);
            }
            else
            {
                LogMgr.instance.Log(LogLevel.ERROR, LogTag.None, "devindzhang itemTweener is not case:" + itemTweener);
            }

        }
    }

    public void ShowEnterFull()
    {
        TweenAlpha tweenAlpha= gameObject.AddMissingComponent<TweenAlpha>();
        tweenAlpha.from = 0.5f;
        tweenAlpha.to = 1;
        tweenAlpha.mTrans = gameObject.transform;
       // tweenAlpha.animationCurve = AnimationCurve.EaseInOut();
        UITweener.Begin<TweenAlpha>(gameObject, 0.15f);
    }


    public void ShowExitFull()
    {
        TweenAlpha tweenAlpha = gameObject.AddMissingComponent<TweenAlpha>();
        tweenAlpha.enabled = false;
        tweenAlpha.mTrans = gameObject.transform;
        // tweenAlpha.animationCurve = AnimationCurve.EaseInOut();
        TweenAlpha tween = TweenAlpha.Begin(gameObject,0.15f, 0);
        tween.onFinished.Clear();
        tween.AddOnFinished(FinishExitAnimation);
        playingQueue.Enqueue(tween);
    }

    public void ShoEnterPopBig()
    {
        TweenAlpha tweenAlpha = gameObject.AddMissingComponent<TweenAlpha>();
        tweenAlpha.enabled = false;
        tweenAlpha.mTrans = gameObject.transform;
        tweenAlpha.value = 0;
        tweenAlpha = TweenAlpha.Begin(gameObject, 0.15f, 1);
        tweenAlpha.onFinished.Clear();
        tweenAlpha.AddOnFinished(FinishEnterAnimation);
        playingQueue.Enqueue(tweenAlpha);


        TweenScale tweenScale = gameObject.AddMissingComponent<TweenScale>();
        tweenScale.enabled = false;
        tweenScale.mTrans = gameObject.transform;
        AnimationCurve curve = new AnimationCurve();
        Keyframe frame = new Keyframe(0, 0, 0, 0);
        Keyframe frame2 = new Keyframe(1, 1, 2, 2);
        curve.AddKey(frame);
        curve.AddKey(frame2);
        curve.postWrapMode = WrapMode.ClampForever;
        curve.preWrapMode = WrapMode.ClampForever;
        tweenScale.animationCurve = curve;

        tweenScale.value = Vector3.zero;
        tweenScale = TweenScale.Begin(gameObject, 0.15f, Vector3.one);
        tweenScale.onFinished.Clear();
        tweenScale.AddOnFinished(FinishEnterAnimation);
        playingQueue.Enqueue(tweenScale);
    }

    public bool IsPlayFinish
    {
        get { return readyPlaying.Count <= 0 && playingQueue.Count <=0; }
    }
    void PlayEnterAnimation()
    {
        if (playingQueue.Count > 0)
        {
            readyPlaying.Enqueue(PlayEnterAnimation);
            return;
        }
        playingQueue.Clear();
        if (EnterPosAnimation != null && EnterPosAnimation.Length > 0 &&
            enterPosDuration != null && enterPosDuration.Length >= EnterPosAnimation.Length &&
            enterPosTo != null && enterPosTo.Length >= EnterPosAnimation.Length)
        {
            for (int i = 0; i < EnterPosAnimation.Length; ++i)
            {
                if (EnterPosAnimation[i] != null)
                {
                    TweenPosition tween = TweenPosition.Begin(EnterPosAnimation[i].gameObject, enterPosDuration[i], enterPosTo[i]);
                    tween.from = enterPosFrom[i];
                    tween.animationCurve = enterPosCurve[i];
                    tween.onFinished.Clear();
                    tween.AddOnFinished(FinishEnterAnimation);
                    playingQueue.Enqueue(tween);
                }
            }
        }

        if (EnterScaleAnimation != null && EnterScaleAnimation.Length > 0 &&
            enterScaleDuration != null && enterScaleDuration.Length >= EnterScaleAnimation.Length &&
            enterScaleTo != null && enterScaleTo.Length >= EnterScaleAnimation.Length)
        {
            for (int i = 0; i < EnterScaleAnimation.Length; ++i)
            {
                if (EnterScaleAnimation[i] != null)
                {
                    TweenScale tween = TweenScale.Begin(EnterScaleAnimation[i].gameObject, enterScaleDuration[i], enterScaleTo[i]);
                    tween.from = enterScaleFrom[i];
                    tween.animationCurve = enterScaleCurve[i];
                    tween.onFinished.Clear();
                    tween.AddOnFinished(FinishEnterAnimation);
                    playingQueue.Enqueue(tween);
                }
            }
        }

        if (EnterAlphaAnimation != null && EnterAlphaAnimation.Length > 0 &&
            enterAlphaDuration != null && enterAlphaDuration.Length >= EnterAlphaAnimation.Length &&
            enterAlphaTo != null && enterAlphaTo.Length >= EnterAlphaAnimation.Length)
        {
            for (int i = 0; i < EnterAlphaAnimation.Length; ++i)
            {
                if (EnterAlphaAnimation[i] != null)
                {
                    TweenAlpha tween = TweenAlpha.Begin(EnterAlphaAnimation[i].gameObject, enterAlphaDuration[i], enterAlphaTo[i]);
                    tween.from = enterAlphaFrom[i];
                    tween.animationCurve = enterAlphaCurve[i];
                    tween.onFinished.Clear();
                    tween.AddOnFinished(FinishEnterAnimation);
                    playingQueue.Enqueue(tween);
                }
            }
        }

        if(EnterRotAnimation != null && EnterRotAnimation.Length > 0 &&
            enterRotDuration != null && enterRotDuration.Length >= EnterRotAnimation.Length &&
            enterRotTo != null && enterRotTo.Length >= EnterRotAnimation.Length)
        {
            for (int i = 0; i < EnterRotAnimation.Length; ++i)
            {
                if (EnterRotAnimation[i] != null)
                {
                    TweenRotation tween = TweenRotation.Begin(EnterRotAnimation[i].gameObject, enterRotDuration[i], Quaternion.Euler(enterRotTo[i]));
                    tween.from = enterRotFrom[i];
                    tween.animationCurve = enterRotCurve[i];
                    tween.onFinished.Clear();
                    tween.AddOnFinished(FinishEnterAnimation);
                    playingQueue.Enqueue(tween);
                }
            }
        }

        //当PlayingQueue为0时，表示没有Tween动画，需要直接退出，以保证之前的 UICamera.ProhibitUI = true 造成的UI锁屏
        if (playingQueue.Count <= 0)
        {
            FinishEnterAnimation();
        }
    }

    void FinishEnterAnimation()
    {
        if (playingQueue.Count > 0)
            playingQueue.Dequeue();
        if (playingQueue.Count > 0)
        {
            return;
        }

        if (readyPlaying.Count > 0)
        {
            EventDelegate.Callback call = readyPlaying.Dequeue();
            call();
        }
        else
        {
            if (finishAniCallBack != null)
                finishAniCallBack();
        }
    }
    /// <summary>
    /// 隐藏时的回调方法
    /// </summary>
    private EventDelegate.Callback exitCallBack;
    public bool OnHide(EventDelegate.Callback callBack, bool isWaitForPlay = true) 
    {
        exitingAnimations.Add(this);
        playingQueue.Clear();
        ShowExitFull();
       
        if (playingQueue.Count > 0 && !isWaitForPlay) return false;
        exitCallBack = callBack;
        PlayExitAnimation();
        return true;
    }

    void PlayExitAnimation()
    {
        if (playingQueue.Count > 0)
        {
            readyPlaying.Enqueue(PlayExitAnimation);
            return;
        }        
        if (ExitPosAnimation != null && ExitPosAnimation.Length > 0)
        {
            for (int i = 0; i < ExitPosAnimation.Length; ++i)
            {
                if (ExitPosAnimation[i] != null)
                {
                    TweenPosition tween = TweenPosition.Begin(ExitPosAnimation[i].gameObject, exitPosDuration[i], exitPosTo[i]);
                    tween.from = exitPosFrom[i];
                    tween.animationCurve = exitPosCurve[i];
                    tween.onFinished.Clear();
                    tween.AddOnFinished(FinishExitAnimation);
                    playingQueue.Enqueue(tween);
                }
            }
        }

        if (ExitScaleAnimation != null && ExitScaleAnimation.Length > 0)
        {
            for (int i = 0; i < ExitScaleAnimation.Length; ++i)
            {
                if (ExitScaleAnimation[i] != null)
                {
                    TweenScale tween = TweenScale.Begin(ExitScaleAnimation[i].gameObject, exitScaleDuration[i], exitScaleTo[i]);
                    tween.from = exitScaleFrom[i];
                    tween.animationCurve = exitScaleCurve[i];
                    tween.onFinished.Clear();
                    tween.AddOnFinished(FinishExitAnimation);
                    playingQueue.Enqueue(tween);
                }
            }
        }

        if (ExitAlphaAnimation != null && ExitAlphaAnimation.Length > 0)
        {
            for (int i = 0; i < ExitAlphaAnimation.Length; ++i)
            {
                if (ExitAlphaAnimation[i] != null)
                {
                    TweenAlpha tween = TweenAlpha.Begin(ExitAlphaAnimation[i].gameObject, exitAlphaDuration[i], exitAlphaTo[i]);
                    tween.from = exitAlphaFrom[i];
                    tween.animationCurve = exitAlphaCurve[i];
                    tween.onFinished.Clear();
                    tween.AddOnFinished(FinishExitAnimation);
                    playingQueue.Enqueue(tween);
                }
            }
        }

        if (ExitRotAnimation != null && ExitRotAnimation.Length > 0 &&
            exitRotDuration != null && exitRotDuration.Length >= ExitRotAnimation.Length &&
            exitRotTo != null && exitRotTo.Length >= ExitRotAnimation.Length)
        {
            for (int i = 0; i < ExitRotAnimation.Length; ++i)
            {
                if (ExitRotAnimation[i] != null)
                {
                    TweenRotation tween = TweenRotation.Begin(ExitRotAnimation[i].gameObject, exitRotDuration[i], Quaternion.Euler(exitRotTo[i]));
                    tween.from = exitRotFrom[i];
                    tween.animationCurve = exitRotCurve[i];
                    tween.onFinished.Clear();
                    tween.AddOnFinished(FinishEnterAnimation);
                    playingQueue.Enqueue(tween);
                }
            }
        }

       if (playingQueue.Count <= 0)
       {
           FinishExitAnimation();
       }
    }

    void FinishExitAnimation()
    {
        if (playingQueue.Count > 0)
            playingQueue.Dequeue();
        if (playingQueue.Count > 0)
        {
            return;
        }

        if (readyPlaying.Count > 0)
        {
            EventDelegate.Callback call = readyPlaying.Dequeue();
            call();
        }
        else if (exitCallBack != null)
        {
            exitCallBack();
        }
        ClearPreExit();
    }

    private void ClearPreExit()
    {
        if(exitingAnimations.Count>0)
        {
            UITweenAnimation uiTweenAnimation = exitingAnimations[0];
            if(uiTweenAnimation != this)
            {
                if(uiTweenAnimation.exitCallBack!=null)
                {
                    uiTweenAnimation.exitCallBack();
                }
                exitingAnimations.RemoveAt(0);
                ClearPreExit();
            }
            else
            {
                exitingAnimations.Remove(this);
            }
        }

    }


    void Update()
	{
		if (playingQueue.Count > 0)
		{
            //MAMI.RefreshRateCtrl.inst.OnRequestHighRate();
            //MAMI.RefreshRateCtrl.inst.RequestUIRebuild();
		}
	}
}
