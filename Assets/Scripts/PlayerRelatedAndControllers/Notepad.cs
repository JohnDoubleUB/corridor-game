﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Notepad : MonoBehaviour
{
    public GameObject linePrefab;
    public GameObject currentLine;
    public List<Vector3> linePositions;
    public bool CanWrite; // Animation related
    public bool PencilOverPadArea; //To Keep track of if the pencil is over the pad

    public AudioSource pencilDrawingSource;
    public AudioClip eraseLineSound;
    public AudioClip drawingSound;

    private bool isWriting;

    public LineRenderer lineRenderer;

    private Vector3 currentPlayerLinePosition;
    private Vector3 lastFrameLinePosition;

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private List<WritingObject> writingObjects = new List<WritingObject>();
    
    public CustomNotepadStarts customStartObject;


    private void Update()
    {

        if (!GameManager.current.playerController.canMove && CanWrite && PencilOverPadArea)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                CreateLine();
            }
            if (Input.GetButton("Fire1"))
            {
                Vector3 tempLinePos = currentPlayerLinePosition;
                if (Vector3.Distance(tempLinePos, linePositions[linePositions.Count - 1]) > .01f)
                {
                    UpdateLine(tempLinePos);
                }
                isWriting = true;
            }
            else
            {
                isWriting = false;
            }

            if (Input.GetKeyDown(KeyCode.Mouse1) && !isWriting)
            {
                if (writingObjects.Any())
                {
                    WritingObject closestWriting = writingObjects.OrderBy(x => Vector3.Distance(currentPlayerLinePosition, x.Centroid)).First();
                    if (closestWriting != null)
                    {
                        writingObjects.Remove(closestWriting);
                        Destroy(closestWriting.LineRenderer.gameObject);
                        transform.PlayClipAtTransform(eraseLineSound, false);
                        lineRenderers.Remove(closestWriting.LineRenderer);
                        linePositions.Clear();
                        //AudioManager.current.PlayClipAt(eraseLineSound, transform.position, 1f, true);
                    }
                }
            }


            PlayDrawingSound(isWriting && lastFrameLinePosition != currentPlayerLinePosition);
            lastFrameLinePosition = currentPlayerLinePosition;

        }

    }

    public void PlayDrawingSound(bool playSound = true)
    {
        pencilDrawingSource.transform.localPosition = currentPlayerLinePosition;

        if (playSound)
        {
            if (!pencilDrawingSource.isPlaying)
            {
                pencilDrawingSource.clip = drawingSound;
                pencilDrawingSource.pitch = Random.Range(0.85f, 1f);
                pencilDrawingSource.Play();
            }
        }
        else
        {
            pencilDrawingSource.Stop();
        }
    }

    public void DrawAt(Vector3 drawlocation)
    {
        currentPlayerLinePosition = drawlocation - transform.position;
    }

    public void ClearCurrentLine()
    {
        if (lineRenderer != null)
        {
            lineRenderers.Add(lineRenderer);
            lineRenderer = null;
        }
    }

    private void CreateLine()
    {
        currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        currentLine.transform.SetParent(transform);
        currentLine.transform.localPosition = Vector3.zero;
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        linePositions.Clear();
        linePositions.Add(currentPlayerLinePosition);
        linePositions.Add(currentPlayerLinePosition);
        lineRenderer.SetPosition(0, linePositions[0]);
        lineRenderer.SetPosition(1, linePositions[1]);
        lineRenderers.Add(lineRenderer);
        writingObjects.Add(new WritingObject(lineRenderer));
    }

    //For loading existing lines from a save
    private void CreateLine(NotepadLineData lineData)
    {
        IEnumerable<Vector3> linePoints = lineData.Positions.Select(x => x.Deserialized());
        if (linePoints == null || !linePoints.Any()) return;


        GameObject newLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        newLine.transform.SetParent(transform);
        newLine.transform.localPosition = Vector3.zero;
        newLine.transform.localRotation = Quaternion.Euler(lineData.LocalRotationEuler.Deserialized());
        newLine.transform.localScale = lineData.LocalScale.Deserialized();

        LineRenderer newLineRenderer = newLine.GetComponent<LineRenderer>();
        newLineRenderer.positionCount = linePoints.Count();
        newLineRenderer.SetPositions(linePoints.ToArray());
        lineRenderers.Add(newLineRenderer);
        writingObjects.Add(new WritingObject(newLineRenderer));
    }

    void UpdateLine(Vector3 newLinePos)
    {
        if (lineRenderer == null) CreateLine();
        linePositions.Add(newLinePos);
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, newLinePos);
    }

    public void SaveData()
    {
        NotepadData dataToSave = new NotepadData(lineRenderers);
        SaveSystem.SaveNotepad(dataToSave);
        //if (customStartObject != null) customStartObject.Varients.Add(dataToSave);
    }

    public void LoadData()
    {
        if (SaveSystem.TryLoadNotepad(out NotepadData notepadData) && notepadData != null && notepadData.LineData != null && notepadData.LineData.Any())
        {
            LoadNotepadData(notepadData);
        }
    }

    public void LoadRandomNote() 
    {
        if (customStartObject != null && customStartObject.Varients.Any()) 
        {
            LoadNotepadData(customStartObject.Varients[Random.Range(0, customStartObject.Varients.Count)]);
        }
    }

    private void LoadNotepadData(NotepadData notepadData)
    {
        foreach (NotepadLineData lineData in notepadData.LineData)
        {
            CreateLine(lineData);
        }
    }

    private class WritingObject 
    {
        public LineRenderer LineRenderer;

        public WritingObject(LineRenderer lineRenderer) 
        {
            LineRenderer = lineRenderer;
        }

        public Vector3 Centroid 
        {
            get 
            {
                Vector3[] linePointPos = new Vector3[LineRenderer.positionCount];
                int lineCount = LineRenderer.GetPositions(linePointPos);
                Vector3 centroid = Vector3.zero;
                
                foreach (Vector3 point in linePointPos) 
                {
                    centroid += point;
                }

                centroid /= lineCount;

                return centroid;
            }
        }
    }

}
