using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEngine.ParticleSystem;

[RequireComponent(typeof(Collider))]
public class NoteController : MonoBehaviour
{
    public void Move()
    {
        throw new NotImplementedException();
    }
    //Materials
    public Material matNormal;
    public Material matPerfect;
    public Material matGood;
    public Material matRegu;
    public Material matBad;
    public Renderer noteRenderer;
    public Renderer noteRenderer2;
    public GameObject particle;
    //Animator
    public Animator animator;

    public float speed = 5f;
    bool _isHit;
    bool isMissed;
    Material currentMaterial { get => noteRenderer.material; set => noteRenderer.material = value; }

    // Objeto de destino que debe alcanzar la nota
    public Transform target;
    public Transform endPoint;

    public UnityAction OnScore;
    public UnityAction<NoteController,float> OnDestroy;
    
    void Update()
    {
        if (target == null || _isHit) return;

        // Movimiento hacia el objetivo        
        transform.position = Vector3.MoveTowards(transform.position, endPoint.position, SongController.SongSpeed * Time.deltaTime);

        // Destruir la nota si llega al objetivo sin ser tocada
        if (transform.position == endPoint.position)
        {
            OnDestroy(this,0);
        }
    }
    public void NoteInit()
    {
        currentMaterial = matNormal;
        currentMaterial.SetFloat("_Alpha", 1);
        noteRenderer2.material.SetFloat("_Alpha", 1);
        _isHit = false;
    }

    public void HitNote()
    {
        _isHit = true;
        var distance = (transform.position - target.position).magnitude;
        if (particle != null) particle.SetActive(true);
        //Debug.Log("Nota tocada distance " + distance);
        if (distance < 0.05f)
        {
            currentMaterial = matPerfect;
            SongController.Instance.AddScore(ScorePerformance.Perfect, 100);
        }
        else if (distance < 0.1f)
        {
            currentMaterial = matGood;
            SongController.Instance.AddScore(ScorePerformance.Awesome, 90);
        }
        else if (distance < 0.15f)
        {
            currentMaterial = matGood;
            SongController.Instance.AddScore(ScorePerformance.VeryGood, 80);
        }
        else if (distance < 0.25f)
        {
            currentMaterial = matGood;
            SongController.Instance.AddScore(ScorePerformance.NotBad, 70);
        }
        else if (distance < 0.5f)
        {
            currentMaterial = matGood;
            SongController.Instance.AddScore(ScorePerformance.Good, 60);
        }
        else if (distance < 0.75f)
        {
            currentMaterial = matGood;
            SongController.Instance.AddScore(ScorePerformance.Meh, 30);
        }
        else if (distance < 1)
        {
            Debug.Log("Nota tocada Regu!");
            currentMaterial = matRegu;
            SongController.Instance.AddScore(ScorePerformance.NotThatGood, 15);
        }
        else
        {
            Debug.Log("Nota tocada mal!");
            currentMaterial = matBad;
            isMissed = true;
        }

        GetComponent<Collider>().enabled = false;
        DOVirtual.Float(1, 0, 0.2f, (value) => {
            currentMaterial.SetFloat("_Alpha", value);
            noteRenderer2.material.SetFloat("_Alpha", value);
        });
    }
}
