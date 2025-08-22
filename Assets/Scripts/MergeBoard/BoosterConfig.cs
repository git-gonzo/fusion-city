using Assets.Scripts.MergeBoard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BoosterConfig;
public enum BoosterType
{
    None,
    UnlimitedCharges,
    UnlimitedEnergy,
    AutoMerge,
    Scissors,
    LevelUP
}
[System.Serializable]
public class BoosterConfig
{
    public BoosterType boosterType;
    public string boosterTitle;
    public string boosterDescrip;
    public int duration;

    public bool isActionable => duration > 0;
}

public class BoosterState
{
    public BoosterType boosterType;
    public DateTime endTime;
    public PieceDiscovery boosterPiece;

    public bool isActive => endTime > DateTime.Now;
}
