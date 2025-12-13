using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text Title_Text;
    public TMP_Text Description_Text;
    public Button End_Button;

    private void Start()
    {
        gameObject.SetActive(false);
        End_Button.onClick.AddListener(CloseUI);
    }

    // success = true；success = false
    public void Show(bool success, string description)
    {
        gameObject.SetActive(true);

        if (success)
        {
            Title_Text.text = "Mission Success!";
            Title_Text.color = Color.green;
        }
        else
        {
            Title_Text.text = "Mission Failed!";
            Title_Text.color = Color.red;
        }

        Description_Text.text = description;

        // a little animtion
        panel.transform.localScale = Vector3.zero;
        LeanTween.scale(panel, Vector3.one, 0.3f).setEaseOutBack();
    }

    private void CloseUI()
    {
        LeanTween.scale(panel, Vector3.zero, 0.2f)
            .setEaseInBack()
            .setOnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }
}