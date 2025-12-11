using UnityEngine;
using static KeyController;

public class ExitCaller : MonoBehaviour
{
    public delegate void PlayerExit();
    public static event PlayerExit OnPlayerExit;

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log($"Trigger enter by {other.name}");
        //Player has gone through the exit
        OnPlayerExit?.Invoke();
    }
}
