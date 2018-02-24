using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour {
  
    #region Singleton
    static EnvironmentManager instance;
    static public EnvironmentManager GetInstance()
    {
        if (instance == null)
            instance = FindObjectOfType<EnvironmentManager>();
        return instance;
    }
    #endregion

    public float timeOfDay_ = 0.0f;
    public float temperature_ = 10;

    public bool isUnderWater_ = false;
    public bool playerShoot_ = false;
    public bool playerRun_ = false;

    public MeshRenderer skyMeshRenderer;


	public void SetTime(float i_time)
    {
        timeOfDay_ = i_time;
    }
    public void SetTemperature(float i_temperature)
    {
        temperature_ = i_temperature;
    }

    public void SetUnderWater(bool i_underWater)
    {
        isUnderWater_ = i_underWater;
    }
    public void SetShoot(bool i_shoot)
    {
        playerShoot_ = i_shoot;
    }
    public void SetRun(bool i_run)
    {
        playerRun_ = i_run;
    }

    public void SetSkyColor(Color i_color)
    {
        skyMeshRenderer.material.SetColor("_Color", i_color);
    }

}
