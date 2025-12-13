using UnityEngine;
using UnityEngine.UI;

public class HP_Bar : MonoBehaviour
{
    public Image fill;
    public void SetHP(float current,float max)
    {
        float percent= current / max;
        fill.fillAmount = percent;


        //dynamic change
        if (percent<0.3f)
        {
            fill.color=Color.red;
        }
        else if(percent<0.6f)
        {
            fill.color=Color.orange;
        }
        else if (percent < 1f)
        {
            fill.color=Color.yellow;
        }
        else
        {
            fill.color=Color.green;
        }

    }

    
}
