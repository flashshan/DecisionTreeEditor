using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour {

    #region Singleton
    static BGMManager instance;
    static public BGMManager GetInstance()
    {
        if (instance == null)
            instance = FindObjectOfType<BGMManager>();
        return instance;
    }
    #endregion

    public AudioClip dayStart_;
    public AudioClip dayMiddle_;
    public AudioClip dayEnd_;

    BGMState currentBGMState_ = BGMState.BGM_DayStart;

    public void TransitionBGMState(BGMState i_bgmState)
    {
        if (i_bgmState == currentBGMState_)
            return;

        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.Stop();
        switch(i_bgmState)
        {
            case BGMState.BGM_DayStart:
                audioSource.clip = dayStart_;
                break;
            case BGMState.BGM_DayMiddle:
                audioSource.clip = dayMiddle_;
                break;
            case BGMState.BGM_DayEnd:
                audioSource.clip = dayEnd_;
                break;
        }
        currentBGMState_ = i_bgmState;
        audioSource.Play();
    }
}
