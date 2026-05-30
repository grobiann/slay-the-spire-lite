using UnityEngine;

namespace STSLite.Core
{

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AudioManager>();
                    if (_instance == null)
                    {
                        _instance = CoreUtils.CreateSingletonObject<AudioManager>("AudioManager");
                    }
                }
                return _instance;
            }
        }
        private static AudioManager _instance;

        public void SetMasterVolume(float volume)
        {
        }

        public void SetBGMVolume(float volume)
        {
        }

        public void SetSFXVolume(float volume)
        {
        }

    }


}