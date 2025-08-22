using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonCounterNotification : MonoBehaviour
{
    public TextMeshProUGUI counterText;
    public Image bgImage;
    int _counter;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCounter(int value)
    {
        _counter = value;
        counterText.text = value.ToString();
    }
    public void SetCounter(int value, Color color)
    {
        _counter = value;
        counterText.text = value.ToString();
        bgImage.color = color;
    }


}
