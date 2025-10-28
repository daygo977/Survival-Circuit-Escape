using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class MenuSelectFirst : MonoBehaviour
{

    [SerializeField] private Selectable firstSelection;

    void OnEnable()
    {
        // Clear whatever was selected before
        EventSystem.current.SetSelectedGameObject(null);

        // Force select the first button so gamepad navigation works
        if (firstSelection != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelection.gameObject);
        }
    }
}
