using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudsLockedBuilding : MonoBehaviour
{
    [SerializeField] ParticleSystem _particleSystem;

    [Button]
    public void CleanArea()
    {
        var param = _particleSystem.velocityOverLifetime;
        param.radial = 10;
        _particleSystem.Stop();
    }
}
