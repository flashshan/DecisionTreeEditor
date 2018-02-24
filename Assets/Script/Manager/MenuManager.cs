using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    public Slider dayTimeSlider_;
    public Slider temperatureSlider_;

    public Text dayTimeText_;
    public Text temperatureText_;

    public Toggle underWater_;
    public Toggle playerRun_;
    public Toggle playerShoot_;


	// Use this for initialization
	void Start () {
        dayTimeSlider_.onValueChanged.AddListener(DayTimeValueChange);
        temperatureSlider_.onValueChanged.AddListener(TemperatureValueChange);

        underWater_.onValueChanged.AddListener(UnderWaterToggle);
        playerRun_.onValueChanged.AddListener(PlayerRunToggle);
        playerShoot_.onValueChanged.AddListener(PlayerShootToggle);
    }

    void DayTimeValueChange(float i_value)
    {
        float dayTime = dayTimeSlider_.value * 24.0f;
        int hour = Mathf.FloorToInt(dayTime);
        int minute = Mathf.FloorToInt((dayTime - (float)hour) * 60.0f);
        dayTimeText_.text = hour.ToString() + ':' + minute.ToString();
        EnvironmentManager.GetInstance().SetTime(dayTime);
    }
                    
    void TemperatureValueChange(float i_value)
    {
        float temperature = temperatureSlider_.value * 40.0f;
        int temperatureInt = Mathf.FloorToInt(temperature);
        temperatureText_.text = temperatureInt.ToString();
        EnvironmentManager.GetInstance().SetTemperature(temperature);
    }

    void UnderWaterToggle(bool i_value)
    {
        EnvironmentManager.GetInstance().SetUnderWater(i_value);
    }

    void PlayerRunToggle(bool i_value)
    {
        EnvironmentManager.GetInstance().SetRun(i_value);
    }

    void PlayerShootToggle(bool i_value)
    {
        EnvironmentManager.GetInstance().SetShoot(i_value);
    }

}
