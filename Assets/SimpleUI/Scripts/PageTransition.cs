using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PageTransition
{
    [Header("Enter")]
    public UnityEvent Event_Enter;
    [Min(0f)] public float durationEnter = 0f;

    [Header("Exit")]
    public UnityEvent Event_Exit;
    [Min(0f)] public float durationExit = 0f;

    public IEnumerator PlayEnter()
    {
        Event_Enter?.Invoke();
        if (durationEnter > 0f)
            yield return new WaitForSeconds(durationEnter);
    }

    public IEnumerator PlayExit()
    {
        Event_Exit?.Invoke();
        if (durationExit > 0f)
            yield return new WaitForSeconds(durationExit);
    }
}