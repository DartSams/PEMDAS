using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletController : MonoBehaviour
{
    public float moveSpeed = 5f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name != "45ACP Bullet")
        {
            //Debug.Log("Entered collision with " + collision.gameObject.name);
            collision.rigidbody.AddForce(transform.forward * 500);
            Destroy(gameObject.transform.parent.gameObject);

        }
    }

}
