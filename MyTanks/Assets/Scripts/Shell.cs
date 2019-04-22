using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    int damage = 3;
    static public float Speed = 80;
    GameObject tank;
    Vector3 direction;
    Rigidbody rd;
    
    Vector3 StartPos;
    const float Range = 20;

    private bool inited = false;

    public void ShellInit(GameObject T, Vector3 d)
    {
        this.tank = T;
        this.direction = d.normalized * Speed * Time.deltaTime;
        rd = GetComponent<Rigidbody>();
        StartPos = T.GetComponent<Rigidbody>().position;
        inited = true;
    }

    private void Update()
    {
        rd.MovePosition(rd.position + direction);
        if ((rd.position - StartPos).magnitude > Range) Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.Equals(this.tank) && inited)
        {
            if (other.tag == "Tank" && tank != null)
            {
                tank.GetComponent<Tank>().Score += damage;
                other.gameObject.GetComponent<Tank>().BeDamaged(damage*2);
            }
            Destroy(this.gameObject);
        }
    }
}
