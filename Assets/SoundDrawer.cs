using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundDrawer : MonoBehaviour
{
    public float Frequency;
    
    int sampleRate = 44_100;
    int position;
    
    AudioClip clip;
    
    List<Vector2> mousePoints = new List<Vector2>();
    List<Vector2> soundPoints = new List<Vector2>();
    int leftButton;
    AudioSource source;
    
    float leftMost;
    float rightMost;
    
    void Start()
    {
        source = GetComponent<AudioSource>();
        leftButton = (int)MouseButton.LeftMouse;
        leftMost = -Camera.main.orthographicSize * Camera.main.aspect;
        rightMost = -leftMost;
        
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(leftButton))
        {
            mousePoints.Clear();
        }
        
        if (Input.GetMouseButton(leftButton))
        {
            // Record the mouse point
            mousePoints.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }

        if (Input.GetMouseButtonUp(leftButton))
        {
            // Duplicate points backwards
            for (int i = mousePoints.Count - 1; i >= 0; i--)
            {
                mousePoints.Add(mousePoints[i]);
            }
            
            // Convert into sound

            soundPoints.Clear();
            position = 0;
            
            var pointDivision = sampleRate / Frequency / mousePoints.Count;

            // Frequency = 500
            // SampleRate = 41,100
            // Points = 300
    
            // Frequency * Points = Unscaled Samples = 150,000
            //     Unscaled Sample / SampleRates = Sample Multiplier = 3.65

            float totalSamples = Frequency * mousePoints.Count;
            float totalSamplesPerRequired = totalSamples / sampleRate;
            float t = 0;
            for (int i = 0; i < sampleRate; i++)
            {
                t += totalSamplesPerRequired;
                int index0 = (int)t % mousePoints.Count;
                int index1 = Mathf.CeilToInt(t) % mousePoints.Count;
                var point = Vector2.Lerp(mousePoints[index0], mousePoints[index1], t % 1);
                soundPoints.Add(point);
            }

            // for (int i = 0; i < sampleRate; i++)
            // {
            //     // i: 0-sampleRate
            //     // t: 0-mousePoints.Count
            //     float t = (float)i * (mousePoints.Count - 1) / sampleRate;
            //     var point0 = mousePoints[(int)t];
            //     var point1 = mousePoints[Mathf.CeilToInt(t)];
            //     soundPoints.Add(Vector2.Lerp(point0, point1, t % 1));
            // }
            print("SoundPoints: " + soundPoints.Count);
            
            source.Stop();
            clip = AudioClip.Create("Name", sampleRate, 2, sampleRate, true, OnAudioRead, OnAudioSetPosition);
            source.clip = clip;
            source.Play();
        }
    }
    
    
    void OnAudioRead(float[] data)
    {
        for (int i = 0; i < data.Length; i++, position++)
        {
            data[i] = Mathf.InverseLerp(leftMost, rightMost, soundPoints[position % sampleRate].x);
        }
    }
    
    void OnAudioSetPosition(int newPosition)
    {
        position = newPosition;
    }

    void OnDrawGizmos()
    {
        foreach (var point in soundPoints)
        {
            Gizmos.DrawCube(point, Vector3.one * 0.1f);
        }
    }
}
