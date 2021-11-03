using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spot : MonoBehaviour
{
    public Button m_Button;

    public int m_iterator;

    public bool isOccupied;

    // Start is called before the first frame update
    void Start()
    {
        m_Button = GetComponent<Button>();

        m_Button.GetComponentInChildren<Text>().text = "";

        isOccupied = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
