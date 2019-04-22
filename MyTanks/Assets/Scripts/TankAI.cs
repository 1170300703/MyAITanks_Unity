using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class HitData
{
    public GameObject TargetTank;
    public float Distance;
    public float Angle;
};

public class TankAI : Tank
{
    float LookRange = 80;
    float LookAngle = 360;
    int LookAccurate = 60;

    public MyMath.MatrixInToHide InMatrix = new MyMath.MatrixInToHide();
    public MyMath.MatrixHideToOut OutMatrix = new MyMath.MatrixHideToOut();
    public float[] Bias = new float[MyMath.OutNode];

    // Start is called before the first frame update
    void Start()
    {
        Score = 2;
        rd = GetComponent<Rigidbody>();
        //Fire(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!canFire)
        {
            nextFire -= fireRate * Time.deltaTime;
            if (nextFire <= 0) canFire = true;
        }

        float[] output = Outputs();

        Move(output[0]);
        Turn(output[1]);
        Fire(MyMath.Relu(output[2]) >= 1);
    }

    float[] Inputs()
    {
        float[] inputs = new float[MyMath.InNode];
        // 1. 1 if ready to fire
        // 2. distance to closest tank
        // 3. angle to closest tank
        // 4. present speed
        // 5. closest tank speed
        // 6. closest tank rotation

        if (canFire) inputs[0] = 1;
        HitData hit = SelectClosest();

        if(hit!=null)
        {
            inputs[1] = hit.Distance;
            inputs[2] = hit.Angle;
            inputs[4] = hit.TargetTank.GetComponent<Tank>().PresentSpeed;
            inputs[5] = hit.TargetTank.GetComponent<Rigidbody>().rotation.y - this.GetComponent<Rigidbody>().rotation.y;
        }
        inputs[3] = PresentSpeed;


        return inputs;
    }
    /* Network */
    float[] Outputs()
    {
        float[] outputs = new float[3];
        // 1.Move
        // 2.Turn
        // 3.Fire
        outputs = OutMatrix.Multi(InMatrix.Multi(Inputs()));
        for(int i=0; i<3; i++)
        {
            outputs[i] += Bias[i];
        }
        return outputs;
    }

    HitData SelectClosest()
    {
        List<HitData> HitDatas = Look();
        HitData hit = null;
        foreach(HitData h in HitDatas)
        {
            if (hit == null || h.Distance < hit.Distance) hit = h;
        }
        return hit;
    }

    List<HitData> Look()
    {
        List< HitData > datas = new List<HitData>();

        HitData hd = LookAround(Quaternion.identity, Color.green);
        if (hd != null)
        {
            datas.Add(hd);
        }

        float subAngle = (LookAngle / 2) / LookAccurate;
        for (int i = 0; i < LookAccurate; i++)
        {
            if ((hd = LookAround(Quaternion.Euler(0, -1 * subAngle * (i + 1), 0), Color.green)) != null
                || (hd = LookAround(Quaternion.Euler(0, subAngle * (i + 1), 0), Color.green)) != null)
                datas.Add(hd);
        }

        return datas;
    }

    HitData LookAround(Quaternion eulerAnger, Color DebugColor)
    {
        Debug.DrawRay(transform.position, eulerAnger * transform.forward.normalized * LookRange, DebugColor);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, eulerAnger * transform.forward, out hit, LookRange) && hit.collider.CompareTag("Tank"))
        {
            HitData data = new HitData();

            data.TargetTank = hit.collider.gameObject;
            data.Distance = (data.TargetTank.transform.position - transform.position).magnitude;
            data.Angle = eulerAnger.eulerAngles.y;

            return data;
        }
        return null;
    }

    public override void BeDamaged(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            this.Score -= 5;
            Die();
        }
    }

    public void Die()
    {
        AI_Manager.AIData data = new AI_Manager.AIData(Score, InMatrix, OutMatrix, Bias);
        AI_Manager.AIdatas.Add(data);
        Destroy(this.gameObject);
    }
}
