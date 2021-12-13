using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CreateBoard : MonoBehaviour
{
    public Spot prefab1;
    private int counter = 0;

    public LinkedList<Spot> m_SpotList;
    // Start is called before the first frame update
    void Start()
    {
        m_SpotList = new LinkedList<Spot>();

        for(int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Spot go = Instantiate(prefab1, new Vector3( (-100 + transform.position.x) + i * 100 , (100 + transform.position.y) + j * -100, 0), Quaternion.identity, transform);

                go.m_iterator = counter;
                counter++;

                m_SpotList.AddLast(go);
            }
        }
        
    }

}
