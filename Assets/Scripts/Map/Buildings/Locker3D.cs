using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class Locker3D : MonoBehaviour
{
    public ParticleSystem particles;
    public AudioSource unlockSound;
    public void PlayUnlockAnim()
    {
        particles.Play();
        GetComponent<Animation>().Play();
        UIUtils.DelayedCall(2.5f, HideLockAndShowPointer, this);
        unlockSound.Play();
    }

    public void HideLockAndShowPointer()
    {
        transform.DOScale(0, 0.5f).SetEase(Ease.InBack);
    }
}
