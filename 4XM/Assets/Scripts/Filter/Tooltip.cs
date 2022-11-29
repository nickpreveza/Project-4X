using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tooltip : MonoBehaviour
{
    TextMeshProUGUI headerField;
    [Multiline()]
    TextMeshProUGUI bodyField;
    LayoutElement layoutElement;
    [SerializeField] int characterWrapLimit;
    bool setup;
    Vector3 tooltipOffset;
    // Start is called before the first frame update

    void Setup()
    {
        if (!setup)
        {
            headerField = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            bodyField = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            layoutElement = GetComponent<LayoutElement>();
            setup = true;
        }
      
    }

    private void Update()
    {
        Vector2 position = Input.mousePosition;
        transform.position = position;
    }

    public void SetData(string body, string header = "")
    {
        if (!setup)
        {
            Setup();
        }

        if (string.IsNullOrEmpty(header))
        {
            headerField.gameObject.SetActive(false);
        }
        else
        {
            headerField.gameObject.SetActive(true);
        }

        headerField.text = header;
        bodyField.text = body;
    }

    void CheckWrap()
    {
        layoutElement.enabled = (headerField.text.Length > characterWrapLimit || bodyField.text.Length > characterWrapLimit) ? true : false;
    }

}
