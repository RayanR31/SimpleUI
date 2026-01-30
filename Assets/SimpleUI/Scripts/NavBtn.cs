using System;
using UnityEngine;
using UnityEngine.UI;

// NavBtn = helper component that turns a standard Unity UI Button
// into a "navigation button" for the ManagerUI system.
//
// It allows designers to:
// - Choose an action (OpenTo, OpenOverlay, Back, etc.) directly in the Inspector.
// - Optionally provide an id (for actions that target a specific page).
// - Avoid writing any custom code or wiring events manually in the Button inspector.
//
// This component requires a Button component on the same GameObject.
[RequireComponent(typeof(Button))]
public class NavBtn : MonoBehaviour
{
    // Id of the target page (used by OpenTo / OpenOverlay actions).
    // Ignored for actions like Back, CloseAll, OpenDefault.
    [SerializeField] private string id;

    // Cached reference to the Unity UI Button component.
    private Button _btn;

    // The navigation action this button will trigger when clicked.
    // Exposed in the Inspector so a designer can choose the behavior.
    public Actions action = Actions.OpenTo;

    // List of all supported navigation actions.
    public enum Actions
    {
        // Open a page as the main (exclusive) page.
        OpenTo,

        // Open a page as an overlay (popup) on top of the current main page.
        OpenOverlay,

        // Go back: close top overlay first, otherwise go back in main page history.
        Back,

        // Close everything and clear all navigation stacks.
        CloseAll,

        // Open the default (root) page defined in ManagerUI.
        OpenDefault
    }

    private void Start()
    {
        // Initialize and bind the button callback.
        InitBtn(); 
    }

    // Gets the Button component and registers the click listener.
    private void InitBtn()
    {
        _btn = GetComponent<Button>();

        // When the button is clicked, call OnClick().
        _btn.onClick.AddListener(OnClick);
    }

    // Called when the Unity UI Button is clicked.
    private void OnClick()
    {
        // Get the global UI manager instance.
        var ui = ManagerUI.Instance;

        // Execute the selected navigation action.
        switch (action)
        {
            case Actions.OpenTo: 
                // Open the target page as the main page.
                ui.OpenTo(id); 
                break;

            case Actions.OpenOverlay: 
                // Open the target page as an overlay (popup).
                ui.OpenOverlay(id); 
                break;

            case Actions.Back: 
                // Perform a back navigation (close overlay or go back in main history).
                ui.Back(); 
                break;

            case Actions.CloseAll: 
                // Close all pages and reset navigation.
                ui.CloseAll(); 
                break;

            case Actions.OpenDefault: 
                // Open the default (root) page.
                ui.OpenDefault(); 
                break;
        }
    }
}
