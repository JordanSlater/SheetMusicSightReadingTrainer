using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class NoteListener: MonoBehaviour
{
    [Tooltip("I suspect that without this delay the microphone is loading data to the clip at the same time as it's reading it and this causes a robotic sound. The delay seems to fix this.")]
    public float audioDelay = 0.00f;

    private string MicrophoneDevice = null;
    private const int recordTimeInSeconds = 1800;
    private AudioSource audioSource;

    private const int SAMPLE_SIZE = 1 << 10;
    private float[] samples = new float[SAMPLE_SIZE];
    private const int EXTRA_POINTS = 7;
    private Vector3[] bars = new Vector3[SAMPLE_SIZE + EXTRA_POINTS];
    private LineRenderer lineRenderer;

    public float MAX_DB_DISPLAY = 100;

    private bool hadFocus = true;

    private bool logged = false;

    private bool ResetAudio()
    {
        Microphone.GetDeviceCaps(MicrophoneDevice, out int minFreq, out int maxFreq);
        audioSource.clip = null;
        try {
            audioSource.clip = Microphone.Start(MicrophoneDevice, loop: true, recordTimeInSeconds, maxFreq);
        } catch {
            return false;
        }
        audioSource.Stop();
        audioSource.PlayDelayed(audioDelay);
        return true;
        /*
         * Next steps:
         * Phone seems to have different update ratio. Maybe I should be using a different log?
         * Get graph working by taking the log.
         **/
    }

    // Start is called before the first frame update
    void Start()
    {
        MicrophoneDevice = Microphone.devices.First();
        audioSource = GetComponent<AudioSource>();
        var first = MicrophoneDevice;
        bool successful = ResetAudio();
        while (!successful)
        {
            successful = ChangeMic();
            if (first == MicrophoneDevice)
            {
                Debug.LogError("Could not find working microphone device.");
                break;
            }
        }

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = SAMPLE_SIZE;
    }

    // Returns true if new mic was successfully reset.
    public bool ChangeMic()
    {
        int length = Microphone.devices.Length;
        int deviceIndex = -1;
        for (int i = 0; i < length; i++)
        {
            if (Microphone.devices.ElementAt(i) == MicrophoneDevice)
            {
                deviceIndex = i;
                break;
            }
        }
        if (deviceIndex == -1)
        {
            Debug.LogError("Current Device isn't used. Defaulting to first device.");
        }
        deviceIndex = (deviceIndex + 1) % length;
        MicrophoneDevice = Microphone.devices.ElementAt(deviceIndex);
        return ResetAudio();
    }

    public void ChangeMicClick()
    {
        if (ChangeMic())
        {
            Debug.Log("Successfully changed to: " + MicrophoneDevice);
        }
        else
        {
            Debug.Log("Problem with changing to: " + MicrophoneDevice);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!Microphone.IsRecording(MicrophoneDevice) && !logged)
        {
            logged = true;
            Debug.Log("Not Recording");
        }

        audioSource.GetSpectrumData(samples, 0, FFTWindow.Rectangular);

        float scale = 500f / (float)Math.Min(Screen.width, Screen.height);

        bars[0].Set(0, scale, 0);
        bars[1].Set(-scale, scale, 0);
        bars[2].Set(-scale, -scale, 0);
        bars[3].Set(0, -scale, 0);
        bars[4].Set(-scale, -scale, 0);
        bars[5].Set(-scale, 0, 0);
        bars[6].Set(0, 0, 0);

        for (int i = 0; i < SAMPLE_SIZE; i++)
        {
            var value = 20 * (float)Math.Log10(samples[i]);
            value = Mathf.Clamp(value, -MAX_DB_DISPLAY, MAX_DB_DISPLAY) / MAX_DB_DISPLAY;
            bars[i + EXTRA_POINTS].Set(10 * ((float)i / (float)SAMPLE_SIZE) * scale, value * scale, 0);
        }
        lineRenderer.SetPositions(bars);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !hadFocus)
        {
            ResetAudio();
        }
        hadFocus = hasFocus;
    }

    private void OnDestroy()
    {
        Microphone.End(MicrophoneDevice);
    }
}
