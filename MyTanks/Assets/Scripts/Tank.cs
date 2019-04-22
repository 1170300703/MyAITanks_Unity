using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    //static public List<Tank> tanks = new List<Tank>();
    public Rigidbody rd { get; protected set; }

    public GameObject Shell;

    public int Score = 0;
    public int Health = 5;
    public const float MoveSpeed = 10;
    public const float TurnSpeed = 360;

    public float fireRate = 5;
    protected float nextFire;
    protected bool canFire = true;
    public float PresentSpeed { get; protected set; }

    // Start is called before the first frame update
    void Start()
    {
        rd = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected void Move(float Forward)
    {
        if (Forward > 1) Forward = 1;
        else if (Forward < -1) Forward = -1;
        rd.MovePosition(transform.forward * Forward * MoveSpeed * Time.deltaTime + rd.position);
        PresentSpeed = Forward;
    }

    protected void Turn(float Right)
    {
        if (Right > 1) Right = 1;
        else if (Right < -1) Right = -1;
        rd.MoveRotation(Quaternion.Euler(0, Right * TurnSpeed * Time.deltaTime, 0) * rd.rotation);
    }

    public virtual void BeDamaged(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            this.Score -= 5;

            Destroy(this.gameObject);
        }
    }

    protected void Fire(bool flag)
    {
        if(flag && canFire)
        {
            GameObject shell = Instantiate(Shell, transform.position + rd.transform.forward.normalized + new Vector3(0,1.5f,0), transform.rotation);
            shell.GetComponent<Shell>().ShellInit(this.gameObject, rd.transform.forward);
            canFire = false;
            nextFire = 1;
        }
    }
}
