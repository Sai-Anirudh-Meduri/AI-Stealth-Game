using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HotbarSelector: MonoBehaviour
{
    public Image[] slots;
    public GameObject[] selected;

    private int currentIndex =0;

    void Start()
    {
        UpdateSelection();
    }

    void Update()
    {
        //9 slot
        var keyboard = Keyboard.current;
        
        if (keyboard == null)
        {
            return;
        }

        for (int i =0;i<slots.Length;i++)
        {
            Key key =Key.Digit1+i;

            if (keyboard[key].wasPressedThisFrame)
            {
                currentIndex=i;
                UpdateSelection();
            }
        }
    }

    public void UpdateSelection()
    {
        for (int i =0; i<slots.Length;i++)
        {
            if (selected[i]!=null)
            {
                selected[i].SetActive(i==currentIndex);
            }
        }
    }
}