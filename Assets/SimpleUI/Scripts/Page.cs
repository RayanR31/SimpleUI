using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

// Page = component that marks a GameObject as a UI Page managed by ManagerUI.
//
// Responsibilities:
// - Holds the unique id of the page.
// - Automatically registers itself to ManagerUI at startup.
// - Automatically unregisters itself when destroyed.
//
// This allows pages to be completely "plug and play":
// just add this component to a UI root GameObject and it becomes manageable by the UI system.
public class Page : MonoBehaviour
{
    // Unique identifier of this page.
    // If left empty in the Inspector, it will default to the GameObject name.
    [SerializeField] private string id;

    public PageTransition pageTransition = new PageTransition();
    // Called once at startup.
    void Start()
    {
        // If no id is provided, use the GameObject name as a default id.
        // This makes setup faster and avoids mandatory manual typing.
        if (string.IsNullOrEmpty(id))
        {
            id = gameObject.name; 
        }

        // Register this page in the ManagerUI so it can be opened/closed by id.
        ManagerUI.Instance.AddElement(id, this);
    }

    // Called when this GameObject is destroyed.
    private void OnDestroy()
    {
        // Unregister this page from the ManagerUI to avoid keeping a dead reference.
        ManagerUI.Instance.RemoveElement(id);
    }
    public void OnEnter()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(pageTransition.IEOnEnter());
    }
    public void OnExit()
    {
        if(gameObject.activeInHierarchy)
            StartCoroutine(pageTransition.IEOnExit(gameObject));
    }
}
