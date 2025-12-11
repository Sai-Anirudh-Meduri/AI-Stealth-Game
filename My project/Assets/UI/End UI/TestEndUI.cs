using UnityEngine;

public class TestEndUI : MonoBehaviour
{
    public EndUI endUI;

    private void Start()
    {
        Debug.Log("Test Start");

        
        Invoke(nameof(TestSuccess), 2f);

        
        Invoke(nameof(TestFail), 5f);
    }

    void TestSuccess()
    {
        Debug.Log("Show Success UI");
        endUI.Show(true, "You successfully completed the mission!");
    }

    void TestFail()
    {
        Debug.Log("Show Failed UI");
        endUI.Show(false, "You failed the mission. Try again!");
    }
}
