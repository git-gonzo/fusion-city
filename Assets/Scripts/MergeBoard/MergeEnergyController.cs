using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Scripts.MergeBoard;
using static UnityEngine.AddressableAssets.Addressables;
using System;
using DG.Tweening;

public class MergeEnergyController : MonoBehaviour
{
    public GameObject btnAddEnergy;
    public TextMeshProUGUI txtEnergy;
    public TextMeshProUGUI txtNextEnergy;
    public Transform boosterEnergyContainer;
    public TextMeshProUGUI txtBoosterEnergyTimeLeft;
    public MergeConfig mergeConfig => GameConfigMerge.instance.mergeConfig;
    public MergeBoardModel mergeModel => GameManager.Instance.MergeManager.boardModel;
    public int energyRefillTotalSeconds => (mergeConfig.maxEnergy - mergeModel.energy) * refillTime;

    public int refillTime;

    private MovingPiece _energyPiece;
    public void ShowEnergyBoosterAnimated()
    {
        ShowEnergyBooster();
        transform.DOPunchScale(Vector3.one * 0.2f,0.25f,0);
    }
    public void ShowEnergyBooster()
    {
        CreatePiece();
        btnAddEnergy.SetActive(false);
        boosterEnergyContainer.parent.gameObject.SetActive(true);
    }
    private void CreatePiece()
    {
        GameManager.RemoveChildren(boosterEnergyContainer.gameObject);
        var prefab = mergeConfig.GetPiecePrefab(mergeModel.energyBooster.boosterPiece);
        _energyPiece = Instantiate(prefab, boosterEnergyContainer).GetComponent<MovingPiece>();
        _energyPiece.AnimateBoostered(true);
        _energyPiece.SetConfig(mergeModel.energyBooster.boosterPiece);
    }

    public void EnergyChange(int value)
    {
        if (value < 0 && mergeModel.energy >= mergeConfig.maxEnergy)
        {
            mergeModel.nextEnergy = DateTime.Now.AddSeconds(refillTime);
        }
        mergeModel.energy += value;
        if (mergeModel.energy >= mergeConfig.maxEnergy)
        {
            txtNextEnergy.gameObject.SetActive(false);
            return;
        }
        txtEnergy.text = mergeModel.energy.ToString();
    }

    public void LazyUpdate()
    {
        if (mergeModel.energy >= mergeConfig.maxEnergy)
        {
            //FULL ENERGY
            btnAddEnergy.SetActive(false);
            txtEnergy.text = mergeModel.energy.ToString();
        }
        else
        {
            var timeLeft = mergeModel.nextEnergy - DateTime.Now;
            if (timeLeft.TotalSeconds <= 0)
            {
                var secondsLeft = Math.Abs(timeLeft.TotalSeconds) % refillTime;
                if (Math.Abs(timeLeft.TotalSeconds) > refillTime)
                {
                    //Has been out for a while
                    var energyAmount = Math.Min(mergeConfig.maxEnergy, Math.Max(1, Math.Abs(timeLeft.TotalSeconds) / refillTime));
                    if (mergeModel.energy > mergeConfig.maxEnergy)
                    {
                        //already had more than max, temporary allowed
                    }
                    else if (mergeModel.energy + (int)energyAmount > mergeConfig.maxEnergy)
                    {
                        //Energy exceds
                        energyAmount = mergeConfig.maxEnergy - mergeModel.energy;
                    }
                    EnergyChange((int)energyAmount);
                    mergeModel.nextEnergy = DateTime.Now.AddSeconds(secondsLeft);
                }
                else
                {

                    //Add 1 energy
                    mergeModel.nextEnergy = DateTime.Now.AddSeconds(refillTime);
                    EnergyChange(1);
                }
                timeLeft = mergeModel.nextEnergy - DateTime.Now;
            }
            else
            {
                if (mergeModel.energy >= mergeConfig.maxEnergy)
                {
                    //FULL ENERGY
                    btnAddEnergy.SetActive(false);
                    return;
                }
            }
            btnAddEnergy.SetActive(true);
            txtNextEnergy.gameObject.SetActive(true);
            txtNextEnergy.text = UIUtils.FormatTime(timeLeft.TotalSeconds);
        }
        if (mergeModel.energyBooster != null)
        {
            if (mergeModel.energyBooster.isActive)
            {
                btnAddEnergy.SetActive(false);
                txtBoosterEnergyTimeLeft.text = UIUtils.FormatTime((mergeModel.energyBooster.endTime - DateTime.Now).TotalSeconds);
            }
            else
            {
                mergeModel.energyBooster = null;
                _energyPiece.SuperDoKill();
                _energyPiece.transform.DOScale(0, 0.5f).SetEase(Ease.InBack).OnComplete(()=>boosterEnergyContainer.parent.gameObject.SetActive(false));
                txtBoosterEnergyTimeLeft.text = "";
            }
        }
    }
}
