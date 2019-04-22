using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AI_Manager : MonoBehaviour
{
    public class AIData
    {
        public int score { get; set; }
        public MyMath.MatrixInToHide InMatrix { get; private set; }
        public MyMath.MatrixHideToOut OutMatrix { get; private set; }
        public float[] Bias { get; private set; }

        public AIData(int score, MyMath.MatrixInToHide InMatrix, MyMath.MatrixHideToOut OutMatrix, float[] Bias)
        {
            this.score = score;
            this.InMatrix = InMatrix;
            this.OutMatrix = OutMatrix;
            this.Bias = Bias;
        }
        
        public class AiDataComparer : IComparer<AIData>
        {
            public int Compare(AIData x, AIData y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                
                if (x.score > y.score) return 1;
                if (x.score < y.score) return -1;
                
                return 0;
            }
        }
    }

    int TankAmount = 22;
    float field = 40;

    float mutationField = 0.1f;
    float mutationRate = 0.2f;

    static int Generation = 0;
    public float NewGenTime = 30f;
    private float NextGenTime = 0.0f;

    Text gen;
    Text time;
    
    static public List<AIData> AIdatas;
    
    public GameObject TankPrefab;
    

    // Start is called before the first frame update
    void Start()
    {
        gen = GameObject.Find("Canvas/Generation").GetComponent<Text>();
        time = GameObject.Find("Canvas/NextGenTime").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        NextGenTime -=  Time.deltaTime;

        gen.text = "当前代数：" + Generation;
        time.text = "下一波倒计时：" + NextGenTime;

        if (NextGenTime <= 0)
        {
            Init();
            NextGenTime = NewGenTime;
        }
    }

    void Init()
    {
        if (Generation == 0)
        {
            for (int tki = 0; tki < TankAmount; tki++)
            {
                GameObject tk = Instantiate(TankPrefab, new Vector3(Random.Range(-field, field),0 , Random.Range(-field, field)), Quaternion.Euler(new Vector3(0, Random.Range(-180, 180),0)));
                TankAI tkai = tk.GetComponent<TankAI>();

                for (int i = 0; i < MyMath.InNode; i++)
                {
                    for (int j = 0; j < MyMath.HideNode; j++)
                    {
                        tkai.InMatrix.matrix[i, j] = Random.Range(-1f, 1f);
                    }
                }

                for(int i=0;i<MyMath.HideNode;i++)
                {
                    tkai.InMatrix.Bias[i] = Random.Range(-1f, 1f);
                }

                for (int i = 0; i < MyMath.HideNode; i++)
                {
                    for (int j = 0; j < MyMath.OutNode; j++)
                    {
                        tkai.OutMatrix.matrix[i, j] = Random.Range(-1f, 1f);
                    }
                }

                for (int i = 0; i < MyMath.OutNode; i++)
                {
                    tkai.Bias[i] = Random.Range(-1f, 1f);
                }

            }
            AIdatas = new List<AIData>();
            Generation++;
        }

        else
        {
            GameObject[] lst = GameObject.FindGameObjectsWithTag("Tank");
            foreach (GameObject obj in lst)
            {
                obj.GetComponent<TankAI>().Die();
            }

            AIdatas.Sort(new AIData.AiDataComparer());

            /*防止死循环*/
            //int min = 10000;
            for(int i=0;i<AIdatas.Count;i++)
            {
                if(AIdatas[i].score<=0)
                {
                    AIdatas[i].score = 1;
                }
            }
            /*if(min<0)
            {
                for (int i = 0; i < AIdatas.Count; i++)
                {
                    AIdatas[i].score += -min;
                }
            }*/


            List<AIData> datalist = new List<AIData>();

            int tki;
            

            if(TankAmount > 30)
            {
                datalist.Add(AIdatas[0]);
                datalist.Add(AIdatas[0]);
                datalist.Add(AIdatas[0]);
                datalist.Add(AIdatas[1]);
                datalist.Add(AIdatas[1]);
                datalist.Add(AIdatas[1]);
                datalist.Add(AIdatas[2]);
                datalist.Add(AIdatas[2]);
                datalist.Add(AIdatas[3]);
                datalist.Add(AIdatas[3]);

                for (tki = 4; tki < TankAmount / 4 ; tki++)
                {
                    datalist.Add(AIdatas[tki]);
                }
            }
            else if (TankAmount > 20)
            {
                datalist.Add(AIdatas[0]);
                datalist.Add(AIdatas[0]);
                datalist.Add(AIdatas[0]);
                datalist.Add(AIdatas[1]);
                datalist.Add(AIdatas[1]);

                for (tki = 2; tki < TankAmount / 4; tki++)
                {
                    datalist.Add(AIdatas[tki]);
                }
            }
            else
            {
                for (tki = 0; tki < TankAmount / 4; tki++)
                {
                    datalist.Add(AIdatas[tki]);
                }
            }

            for (; tki < TankAmount; tki++)
            {
                int rb = Random.Range(0, MyMath.OutNode);
                int ran = rb * MyMath.HideNode + Random.Range(0, MyMath.HideNode);
                int hb = Random.Range(0, MyMath.HideNode);

                int t1 = Random.Range(0, 200);
                int t2 = Random.Range(0, 200);

                AIData AI1, AI2;

                MyMath.MatrixInToHide InMatrix = new MyMath.MatrixInToHide();
                MyMath.MatrixHideToOut OutMatrix = new MyMath.MatrixHideToOut();
                float[] Bias = new float[MyMath.OutNode];


                int wheel = 0;

                for (int i = 0; ; i %= TankAmount)
                {
                    wheel += AIdatas[i].score;
                    if (wheel >= t1 && i <= TankAmount)
                    {
                        AI1 = AIdatas[i];
                        break;
                    }
                    i++;
                }
                for (int i = 0; ; i %= TankAmount)
                {
                    wheel += AIdatas[i].score;
                    if (wheel >= t2 && i <= TankAmount)
                    {
                        AI2 = AIdatas[i];
                        break;
                    }
                    i++;
                }
                /* 杂交 */
                /*InMatrix*/
                for(int i=0;i<MyMath.InNode;i++)
                {
                    for(int j=0;j<MyMath.HideNode;j++)
                    {
                        if ((i * MyMath.HideNode + j) < ran)
                        {
                            InMatrix.matrix[i, j] = AI1.InMatrix.matrix[i, j];
                        }
                        else
                        {
                            InMatrix.matrix[i, j] = AI2.InMatrix.matrix[i, j];
                        }
                        //突变
                        if (Random.Range(0, 1f) < mutationRate / 4)
                        {
                            InMatrix.matrix[i, j] += Random.Range(-mutationField * 16 < 1 ? -mutationField * 16 : 1, mutationField * 16 < 1 ? -mutationField * 16 : 1);
                        }

                        if (Random.Range(0, 1f) < mutationRate / 2)
                        {
                            InMatrix.matrix[i, j] += Random.Range(-mutationField * 4, mutationField * 4);
                        }

                        if (Random.Range(0, 1f) < mutationRate)
                        {
                            InMatrix.matrix[i, j] += Random.Range(-mutationField, mutationField);
                        }

                        if (Random.Range(0, 1f) < mutationRate * 2)
                        {
                            InMatrix.matrix[i, j] += Random.Range(-mutationField / 4, mutationField / 4);
                        }
                    }
                }
                /*OutMatrix*/
                for (int i = 0; i < MyMath.OutNode; i++)
                {
                    for (int j = 0; j < MyMath.HideNode; j++)
                    {
                        if ((i * MyMath.HideNode + j) < ran)
                        {
                            OutMatrix.matrix[j, i] = AI1.OutMatrix.matrix[j, i];
                        }
                        else
                        {
                            OutMatrix.matrix[j, i] = AI2.OutMatrix.matrix[j, i];
                        }
                        //突变
                        if (Random.Range(0, 1f) < mutationRate / 4)
                        {
                            OutMatrix.matrix[j, i] += Random.Range(-mutationField * 16 < 1 ? -mutationField * 16 : 1, mutationField * 16 < 1 ? -mutationField * 16 : 1);
                        }
                        if (Random.Range(0, 1f) < mutationRate / 2)
                        {
                            OutMatrix.matrix[j, i] += Random.Range(-mutationField * 4, mutationField * 4);
                        }
                        if (Random.Range(0, 1f) < mutationRate)
                        {
                            OutMatrix.matrix[j, i] += Random.Range(-mutationField, mutationField);
                        }
                        if (Random.Range(0, 1f) < mutationRate * 2)
                        {
                            OutMatrix.matrix[j, i] += Random.Range(-mutationField / 4, mutationField / 4);
                        }
                    }
                }

                /*Hide Bias*/
                for (int i = 0; i < MyMath.HideNode; i++)
                {
                    /*if (i < hb) InMatrix.Bias[i] = AI1.InMatrix.Bias[i];
                    else InMatrix.Bias[i] = AI2.InMatrix.Bias[i];*/

                    if (i == hb) InMatrix.Bias[i] = AI2.InMatrix.Bias[i];
                    else InMatrix.Bias[i] = AI1.InMatrix.Bias[i];

                    if (Random.Range(0, 1f) < mutationRate / 2)
                    {
                        InMatrix.Bias[i] += Random.Range(-mutationField * 2, mutationField * 2);
                    }
                    if (Random.Range(0, 1f) < mutationRate)
                    {
                        InMatrix.Bias[i] += Random.Range(-mutationField, mutationField);
                    }
                    if (Random.Range(0, 1f) < mutationRate * 2)
                    {
                        InMatrix.Bias[i] += Random.Range(-mutationField / 4, mutationField / 4);
                    }
                }
                /*Out Bias*/
                for (int i=0;i<MyMath.OutNode;i++)
                {
                    /*if (i < rb) Bias[i] = AI1.Bias[i];
                    else Bias[i] = AI2.Bias[i];*/

                    if (i == rb) Bias[i] = AI2.Bias[i];
                    else Bias[i] = AI1.Bias[i];

                    if (Random.Range(0, 1f) < mutationRate / 2)
                    {
                        Bias[i] += Random.Range(-mutationField * 2, mutationField * 2);
                    }
                    if (Random.Range(0, 1f) < mutationRate)
                    {
                        Bias[i] += Random.Range(-mutationField, mutationField);
                    }
                    if (Random.Range(0, 1f) < mutationRate * 2)
                    {
                        Bias[i] += Random.Range(-mutationField / 4, mutationField / 4);
                    }
                }
                /* 只突变 交换1位 */
                /*for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (i == rb)
                        {
                            m49.matrix[i, j] = AI2.InMatrix.matrix[i, j];
                            m93.matrix[j, i] = AI2.OutMatrix.matrix[j, i];

                        }
                        else
                        {
                            m49.matrix[i, j] = AI1.InMatrix.matrix[i, j];
                            m93.matrix[j, i] = AI1.OutMatrix.matrix[j, i];
                        }
                        if (Random.Range(0, 1f) < mutationRate)
                        {
                            m49.matrix[i, j] += Random.Range(-mutationField, mutationField);
                        }
                        if (Random.Range(0, 1f) < mutationRate)
                        {
                            m93.matrix[j, i] += Random.Range(-mutationField, mutationField);
                        }
                    }
                    if (i == rb) Bias[i] = AI2.Bias[i];
                    else Bias[i] = AI1.Bias[i];
                    if (Random.Range(0, 1f) < mutationRate)
                    {
                        Bias[i] += Random.Range(-mutationField, mutationField);
                    }
                }*/
            
                datalist.Add(new AIData(0,InMatrix, OutMatrix, Bias));
            }
            
            for (tki = 0; tki < TankAmount; tki++)
            {
                GameObject tk = Instantiate(TankPrefab, new Vector3(Random.Range(-field, field), 0, Random.Range(-field, field)), Quaternion.Euler(new Vector3(0, Random.Range(-180, 180), 0)));
                TankAI tkai = tk.GetComponent<TankAI>();
                {
                    tkai.InMatrix = datalist[tki].InMatrix;
                    tkai.OutMatrix = datalist[tki].OutMatrix;
                    tkai.Bias = datalist[tki].Bias;
                }
            }
            AIdatas.Clear();
            Generation++;
        }
        
    }
}
