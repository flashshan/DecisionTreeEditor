using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    
    #region Singleton
    static CameraManager instance;
    static public CameraManager GetInstance()
    {
        if (instance == null)
            instance = FindObjectOfType<CameraManager>();
        return instance;
    }
    #endregion

    public Light waterLight;

    public Light sunLight;
    public Color sunLightHot;
    public Color sunLightCool;
 
    CameraEffect currentEffectState_;
    CameraState currentCameraState_;

    public Vector3 normalPosition_;
    public Vector3 runPosition_;
    public Vector3 shootPosition_;
    public float lerpTime_;

    Vector3 fromPosition_;
    Vector3 targetPosition_;
    bool isLerping_ = false;
    float timer_;


    public void TransitionEffectState(CameraEffect i_effectState)
    {
        if (currentEffectState_ == i_effectState)
            return;

        switch(i_effectState)
        {
            case CameraEffect.NoWaterCool:
                waterLight.gameObject.SetActive(false);
                sunLight.color = sunLightCool;
                break;
            case CameraEffect.NoWaterHot:
                waterLight.gameObject.SetActive(false);
                sunLight.color = sunLightHot;
                break;
            case CameraEffect.WaterCool:
                waterLight.gameObject.SetActive(true);
                sunLight.color = sunLightCool;
                break;
            case CameraEffect.WaterHot:
                waterLight.gameObject.SetActive(true);
                sunLight.color = sunLightHot;
                break;
        }
        currentEffectState_ = i_effectState;
    }

    public void TransitionToState(CameraState i_cameraState)
    {
        if (currentCameraState_ == i_cameraState)
            return;

        fromPosition_ = transform.position;
        switch(i_cameraState)
        {
            case CameraState.NormalState:
                targetPosition_ = normalPosition_;
                break;
            case CameraState.RunState:
                targetPosition_ = runPosition_;
                break;
            case CameraState.ShootState:
                targetPosition_ = shootPosition_;
                break;
        }
        currentCameraState_ = i_cameraState;

        isLerping_ = true;
        timer_ = 0.0f;
    }

    void Update()
    {
        if(isLerping_)
        {
            timer_ += Time.deltaTime;
            transform.position = Vector3.Lerp(fromPosition_, targetPosition_, timer_ / lerpTime_);

            if (timer_ > lerpTime_)
            {
                transform.position = targetPosition_;
                isLerping_ = false;
            }
        }
    }
}
