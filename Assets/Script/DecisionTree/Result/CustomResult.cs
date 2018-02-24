using UnityEngine;



public class BGMResult : BasicResult
{
    [DtVariable("BGMState", "BGM State")]
    public BGMState bgmState_;

    public BGMResult() : base()
    {
        excuteInterval = 60;
    }

    public override void BeOverrided(BasicResult i_other)
    {
        base.BeOverrided(i_other);

        bgmState_ = (i_other as BGMResult).bgmState_;
    }

    public override void Execute()
    {
        base.Execute();

        BGMManager.GetInstance().TransitionBGMState(bgmState_);
    }
}


public class SkyColorResult : BasicResult
{
    [DtVariable("SkyColor", "Sky Color")]
    public Color skyColor_;


    public SkyColorResult() : base()
    {
        excuteInterval = 10;
    }  // for editor only

    public override void BeOverrided(BasicResult another)
    {
        base.BeOverrided(another);

        skyColor_ = (another as SkyColorResult).skyColor_;
    }

    public override void Execute()
    {
        base.Execute();

        EnvironmentManager.GetInstance().SetSkyColor(skyColor_);
    }
}


public class CameraEffectResult : BasicResult
{
    [DtVariable("CameraEffectState", "Camera Effect State")]
    public CameraEffect effectState_;

    public CameraEffectResult() : base()
    {
        excuteInterval = 10;
    }

    public override void BeOverrided(BasicResult i_other)
    {
        base.BeOverrided(i_other);

        effectState_ = (i_other as CameraEffectResult).effectState_;
    }

    public override void Execute()
    {
        base.Execute();

        CameraManager.GetInstance().TransitionEffectState(effectState_);
    }
}


public class CameraStateResult : BasicResult
{
    [DtVariable("CameraState", "Camera State")]
    public CameraState cameraState_;

    public CameraStateResult() : base()
    {
        excuteInterval = 10;
    }

    public override void BeOverrided(BasicResult i_other)
    {
        base.BeOverrided(i_other);

        cameraState_ = (i_other as CameraStateResult).cameraState_;
    }

    public override void Execute()
    {
        base.Execute();

        CameraManager.GetInstance().TransitionToState(cameraState_);
    }
}