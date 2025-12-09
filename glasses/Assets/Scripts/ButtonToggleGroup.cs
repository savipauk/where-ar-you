using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonToggleGroup : MonoBehaviour
{
    public List<Button> buttonsInGroup = new();
    public Button currentlySelected = null;

    void Start()
    {
        if (buttonsInGroup.Count > 0)
        {
            // Start with the first button selected
            OnButtonClicked(buttonsInGroup[0]);
        }

        foreach (Button button in buttonsInGroup)
        {
            button.onClick.AddListener(() => OnButtonClicked(button));
        }
    }

    void OnButtonClicked(Button clickedButton)
    {
        foreach (Button button in buttonsInGroup)
        {
            if (button == clickedButton)
            {
                // Select the clicked button
                button.interactable = false;
                currentlySelected = button;
            }
            else
            {
                // Deselect other buttons
                button.interactable = true;
            }
        }
    }
}
