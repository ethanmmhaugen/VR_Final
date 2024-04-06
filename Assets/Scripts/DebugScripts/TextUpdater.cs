using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextUpdater : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI debugText;
    // Start is called before the first frame update
    void Start()
    {
        DebugText.log(debugText.text);
        debugText.text = DebugText.TheText;
    }

    // Update is called once per frame
    void Update()
    {
        debugText.text = DebugText.TheText;
    }
}
