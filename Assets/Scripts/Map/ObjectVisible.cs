using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectVisible : MonoBehaviour
{
    public int HideLevelBelow;
    private bool _isHidden = false;
    Renderer[] _renderers;

    // Start is called before the first frame update
    void Start()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        CheckStatus();
    }

    // Update is called once per frame
    void CheckStatus()
    {
        if (!GameManager.Instance) return;
        if (HideLevelBelow > 0)
        {
            if(HideLevelBelow > GameManager.Instance.PlayerLevel && !_isHidden)
            {
                _isHidden = true;
                ShowRenderers(false);
                GameManager.Instance.playerData.OnLevelUp += CheckStatus;
            }
            else if (HideLevelBelow < GameManager.Instance.PlayerLevel && _isHidden)
            {
                _isHidden = false;
                ShowRenderers(true);
            }
            return;
        }
        if (!GameConfigMerge.instance.objectsVisibility) return;
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        if (screenPoint.z > 0 && screenPoint.x > -0.3f && screenPoint.x < 1.3 && screenPoint.y > -0.2f && screenPoint.y < 1.2f)
        {
            ShowRenderers(true);
        }
        else
        {
            ShowRenderers(false);
        }
    }

    private void ShowRenderers(bool value)
    {
        foreach (Renderer r in _renderers)
        {
            if (r != null)
                r.enabled = value;
        }
        gameObject.SetActive(value);
    }
}
