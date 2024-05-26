using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace masterland.Master
{
    public enum AudioType 
    {
        None,
        GetHit,
        SwordHit,
        SwordSlash
    }

    public class Audio : MasterComponent
    {
        [SerializeField] private AudioClip _gethit;
        [SerializeField] private AudioClip _swordHit;
        [SerializeField] private AudioClip _swordSlash;
        [SerializeField] private AudioSource _audioSource;

        public void PlayOneShot(AudioType audioType) 
        {   
            _audioSource.volume =1;
            switch(audioType) 
            {
                case AudioType.GetHit : 
                    _audioSource.PlayOneShot(_gethit);
                    break;
                case AudioType.SwordHit : 
                    _audioSource.PlayOneShot(_swordHit);
                    break;
                case AudioType.SwordSlash :
                     _audioSource.volume =0.2f;
                    _audioSource.PlayOneShot(_swordSlash);
                    break;
            }
        }
    }
}
