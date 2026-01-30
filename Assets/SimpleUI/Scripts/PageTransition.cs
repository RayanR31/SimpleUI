using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PageTransition
{
    [Header("Event Enter")]
    public UnityEvent Event_Enter; 
    public float durationEnter = 0f;

    [Header("Event Exit")]
    public UnityEvent Event_Exit;
    public float durationExit = 0f;
    public IEnumerator IEOnEnter()
    {
        Event_Enter?.Invoke();
        yield return new WaitForSeconds(durationEnter);
    }
    public IEnumerator IEOnExit(GameObject go)
    {
        Event_Exit?.Invoke();
        yield return new WaitForSeconds(durationExit);
        go.SetActive(false);
    }
}
