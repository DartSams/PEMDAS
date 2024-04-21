using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class orbitalMeleeWeapon : MonoBehaviour
{
    public GameObject player;
    public int speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (tag == "weapon")
        {
            transform.RotateAround(player.transform.position,Vector3.up,speed * Time.deltaTime);
        }
    }
}
