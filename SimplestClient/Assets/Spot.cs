using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spot : MonoBehaviour
{
    public Button m_Button;

    public string m_ButtonText;

    public int m_iterator;

    public bool isOccupied;

    // Start is called before the first frame update
    void Start()
    {
        m_Button = GetComponent<Button>();

        m_ButtonText = "";

        m_Button.GetComponentInChildren<Text>().text = m_ButtonText;

        isOccupied = false;
    }

    public void ChangeButonText(string newText)
    {
        m_ButtonText = newText;
        m_Button.GetComponentInChildren<Text>().text = m_ButtonText;
    }

}
