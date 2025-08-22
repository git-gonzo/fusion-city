using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
[CreateAssetMenu(fileName = "SongConfig",menuName = "MusicGame")]
public class SongConfig : ScriptableObject
{
    public AudioResource musicTrack;
    public float timeBeforeSound;
    public float speed;
    public bool useTimeSinceStart;
    public List<SongChunk> chunks;
}



[Serializable]
public class SongChunk
{
    public float timeSinceStart;
    public List<SongStep> steps;
}

[Serializable]
public class SongStep
{
    [HorizontalGroup("A")] public float timing;
    [HorizontalGroup("A")] public int loops;


    [HorizontalGroup, HideLabel, EnumPaging, GUIColor("@this.noteLane1 == NoteType.Empty ? Color.gray : Color.green")]
    public NoteType noteLane1;
    [HorizontalGroup, HideLabel, EnumPaging, GUIColor("@this.noteLane2 == NoteType.Empty ? Color.gray : Color.green")]
    public NoteType noteLane2;
    [HorizontalGroup, HideLabel, EnumPaging, GUIColor("@this.noteLane3 == NoteType.Empty ? Color.gray : Color.green")]
    public NoteType noteLane3;

    
}

public enum NoteType
{
    Empty = 0,
    Normal = 1,
    Medium = 2,
    Small = 3
}

