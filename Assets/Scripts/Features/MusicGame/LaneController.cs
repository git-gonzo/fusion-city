using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class LaneController : MonoBehaviour
{
    // Prefab de nota para este carril
    public NoteController noteBigPrefab;
    public NoteController noteMediumPrefab;
    public NoteController noteSmallPrefab;

    // Tiempo entre la creaci?n de notas (en segundos)
    public float spawnDelay = 1f;
    [MinMaxSlider(0, 5)]
    public Vector2 spawnOffset;

    // Tiempo de vida m?ximo de la nota (en segundos)
    public float noteLifetime = 5f;

    // Posici?n donde se crean las notas
    public Transform spawnPoint;
    public Transform destinationPoint;
    public Transform targetPoint;
    private List<NoteController> notes;

    // Variables privadas para el control interno del script
    private float currentTime = 0f;
    private float spawnTimer = 0f;

    public void InitLane()
    {
        notes = new List<NoteController>();
    }

    public void SpawnNote(NoteType noteType)
    {
        NoteController prefab = noteBigPrefab;
        NoteController newNote;
        Debug.Log("Spawn note, pool count " + SongController.Instance.notesPool.Count);

        if (SongController.Instance.notesPool.Count > 0)
        {
            newNote = SongController.Instance.notesPool.First();
            SongController.Instance.notesPool.RemoveAt(0);
            newNote.transform.position = spawnPoint.position;
            newNote.gameObject.SetActive(true);
            Debug.Log("Note from poool");
        }
        else
        {
            Debug.Log("Instantiate new note");
            newNote = Instantiate(prefab, spawnPoint.position, Quaternion.identity, SongController.Instance.notesContainer);
            newNote.OnDestroy = RemoveNote;
        }
        newNote.target = targetPoint;
        newNote.endPoint = destinationPoint;
        if (noteType == NoteType.Normal)
        {
            newNote.transform.DOScaleZ(1, 0);
        }
        else if (noteType == NoteType.Medium)
        {
            newNote.transform.DOScaleZ(0.75f, 0);
        }
        else if (noteType == NoteType.Small)
        {
            newNote.transform.DOScaleZ(0.5f, 0);
        }
        notes.Add(newNote);
    }

    private void Update()
    {
        if (SongController.Instance.isGameOver) return;
        CheckNoteHit();
    }

    void CheckNoteHit()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Check(Input.mousePosition);
            return;
        }
#endif
        if (Input.touchCount > 0)
        {
            foreach (var touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    Check(touch.position);
                }
            }
        }
    }

    void Check(Vector2 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out var hit))
        {
            if(hit.transform == transform)
            {
                TryToHitNote();
            }
        }
    }

    void TryToHitNote()
    {
        //Find note closest to target
        notes.Sort((a, b) => { return Vector3.Distance(a.transform.position, targetPoint.position).CompareTo(Vector3.Distance(b.transform.position, targetPoint.position)); });
        var note = notes.First();
        var distance = (note.transform.position - targetPoint.position).magnitude;
        if(distance > 2)
        {
            //Debug.Log("distance " + distance);
            return;
        }
        RemoveNote(note,0.2f);
        note.HitNote();
    }

    public async void RemoveNote(NoteController note, float destroyDelay = 0)
    {
        notes.Remove(note);
        await Task.Delay((int)(destroyDelay * 1000));
        SongController.Instance.notesPool.Add(note);
        note.NoteInit();
        note.gameObject.SetActive(false);
        //Destroy(note.gameObject, destroyDelay);
    }
}
