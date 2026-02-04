using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class PageTransition
{
    [Header("Enter Transition")]

    [Tooltip("Invoked when the page becomes visible.\n" +
             "Use this to trigger animations (Animator, CanvasGroup fade, Timeline, etc.).")]
    [SerializeField] private UnityEvent onEnter;

    [Tooltip("Duration of the enter transition in seconds.\n" +
             "ManagerUI waits for this time before allowing new navigation.")]
    [Min(0f)]
    [SerializeField] private float enterDuration = 0f;



    [Header("Exit Transition")]

    [Tooltip("Invoked when the page is about to be hidden.")]
    [SerializeField] private UnityEvent onExit;

    [Tooltip("Duration of the exit transition in seconds.")]
    [Min(0f)]
    [SerializeField] private float exitDuration = 0f;


    public IEnumerator PlayEnter()
    {
        onEnter?.Invoke();
        if (enterDuration > 0f)
            yield return new WaitForSeconds(enterDuration);
    }

    public IEnumerator PlayExit()
    {
        onExit?.Invoke();
        if (exitDuration > 0f)
            yield return new WaitForSeconds(exitDuration);
    }
}