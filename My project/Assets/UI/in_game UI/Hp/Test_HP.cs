using UnityEngine;
using UnityEngine.InputSystem;
public class Test_HP : MonoBehaviour
{
    public HP_Bar hp;
    float current=100;
    float max=100;

    void Update()
    {
         if (Keyboard.current.minusKey.wasPressedThisFrame)
        {
            Debug.Log("Pressed -");
            current -= 5;
            hp.SetHP(current, max);
        }

        if (Keyboard.current.equalsKey.wasPressedThisFrame)
        {
            Debug.Log("Pressed =");
            current += 5;
            if (current > max) current = max;
            hp.SetHP(current, max);
        }
    }
    

}