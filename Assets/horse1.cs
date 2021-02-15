
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class horse1 : MonoBehaviour
{
    public GameObject prefab;
    GameObject[] creature = new GameObject[creatureNumber];
    float time = 0;                   //世代交代までの時間を計る
    float time1 = 0;                 //関節を動かすまでの時間を計る
    float frequency = 0.2F;         //関節を動かす細かさ
    static int creatureNumber = 101;  //個体数は奇数
    bool one = false;
    int k = 10;
    int limitTime = 10;              //limitTimeごとに世代交代
    int lowerLimit = -25;            //関節の角度の下限
    int upperLimit = 35;             //関節の角度の上限
    static int angleNumber = 11;      //関節の数
    static int step = 6;             //step数で運動の一周期
    int leave = 3;                   //残す個体数 奇数
    int stepN = 0;
    float mutationRate = 1;          //突然変異はmutationRate%の割合で起きる
    float[,] angle = new float[creatureNumber, angleNumber];
    float[,,] GA = new float[creatureNumber, step, angleNumber];
    float[,] GAarray = new float[creatureNumber, step * angleNumber];
    float[] distance = new float[creatureNumber];    //歩行距離
	float[] side = new float[creatureNumber];  //　横の移動距離
	/// </summary>
    System.Random r = new System.Random();
    //Random r = new Random();
    // Use this for initialization
    void Start()
    {
        /*GameObject go = Instantiate(prefab) as GameObject;
        HingeJoint hinge = GetComponent<HingeJoint>();
        JointSpring hingeSpring = hinge.spring;
        hingeSpring.spring = 25;
        hingeSpring.damper = 6;
        hingeSpring.targetPosition = 40;
        hinge.spring = hingeSpring;
        hinge.useSpring = true;*/
        for (int i = 0; i < creatureNumber; i++)
        {
            for (int j = 0; j < step; j++)
            {
                for (int l = 0; l < angleNumber; l++)
                {
                    GA[i, j, l] = r.Next(lowerLimit, upperLimit);
                    GAarray[i, 8 * j + l] = GA[i, j, l];
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        time1 += Time.deltaTime;

        initiate();       //一定時間後モデル生成
        if (time1 >= frequency)   //動き方の細かさを決める　デバックではこの値を大きくするとよい
        {
            for (int i = 0; i < creatureNumber; i++)
            {
                for (int l = 0; l < angleNumber; l++)
                {
                    angle[i, l] = GA[i, stepN, l];
                }
            }

            walk();
            time1 = 0;
        }
        select();
        shuffle();
        mutation();
        arrayToGA();
        destroy();
        stepN++;
        if (stepN >= step)
        {
            stepN = 0;
        }



    }

    void mutation()
    {
        if (time >= limitTime && one == true)
        {
            for (int i = leave; i < creatureNumber; i++)
            {
                for (int j = 0; j < step * angleNumber; j++)
                {
                    int a = r.Next(0, 100);
                    if (0 <= a && a <= mutationRate)
                    {
                        int b = r.Next(lowerLimit, upperLimit);
                        GAarray[i, j] = b;
                    }
                }
            }
        }
    }
    void arrayToGA()
    {
        for (int i = 0; i < creatureNumber; i++)
        {
            for (int j = 0; j < step; j++)
            {
                for (int l = 0; l < angleNumber; l++)
                {
                    GA[i, j, l] = GAarray[i, 8 * j + l];
                }
            }
        }
    }

    void shuffle()
    {
        if (time >= limitTime && one == true)
        {
            int first = 0;
            int second = 0;
            int firstCopy = 0;
            float[,] GAarrayCopy = new float[creatureNumber, step * angleNumber];
            copy2(GAarray, GAarrayCopy, creatureNumber, step * angleNumber);
            for (int i = leave; i < creatureNumber; i += 2)
            {
                first = r.Next(1, 30);
                second = r.Next(1, 31);
                while (first == second)
                {
                    second = r.Next(1, 31);
                }
                if (first > second)
                {
                    firstCopy = first;
                    first = second;
                    second = firstCopy;
                }
                for (int j = first; j < second; j++)
                {
                    GAarray[i, j] = GAarrayCopy[i + 1, j];
                    GAarray[i + 1, j] = GAarrayCopy[i, j];
                }
            }
        }
    }

    void init1(float[] a, int x) //一次元配列の初期化
    {
        for (int i = 0; i < x; i++)
        {
            a[i] = 0;
        }
    }

    void print1(float[] a, int x)  //一次元配列の出力
    {
        string result = "";
        for (int i = 0; i < x; i++)
        {
            result += ",";
            result += a[i].ToString();
        }
        print(result);
    }
    void print2(float[,] a, int x, int y)  //二次元配列の出力
    {
        for (int i = 0; i < x; i++)
        {
            string result = "";
            for (int j = 0; j < y; j++)
            {
                result += ",";
                result += a[i, j].ToString();
            }
            print(result);
        }
    }
    void select()
    {
        if (time >= limitTime && one == true)
        {
            float distanceSum = 0;
            float[] distanceTop = new float[leave];
            int[] distanceTopN = new int[leave];
            for (int i = 0; i < leave; i++)
            {
                distanceTop[i] = -100;
                distanceTopN[i] = 0;
            }
            float[] distanceRate = new float[creatureNumber];
            init1(distanceRate, creatureNumber);
            int a = 0;
            float[,] GAarrayCopy = new float[creatureNumber, step * angleNumber];
            copy2(GAarray, GAarrayCopy, creatureNumber, step * angleNumber);
            for (int i = 0; i < creatureNumber; i++)
            {
                distance[i] = creature[i].transform.position.z;
				//side [i] = Mathf.Abs (creature [i].transform.position.x - 10 + 25 * i);
                if (distance[i] < 0)
                {
                    distance[i] = 0;
                }
                distanceSum += distance[i];
                for (int j = i; j < creatureNumber; j++)
                {
					//distanceRate[j] += distance[i]-5*side[i];
					distanceRate[j] += distance[i];
                }
            }

            for (int i = 0; i < creatureNumber; i++)
            {
                distanceRate[i] = distanceRate[i] / distanceSum * 100;
            }
            for (int j = 0; j < leave; j++)
            {
                for (int i = 0; i < creatureNumber; i++) //一番優秀な遺伝子を決める。
                {
                    if (j == 0)
                    {
                        if (distance[i] >= distanceTop[j])
                        {
                            distanceTop[j] = distance[i];
                            distanceTopN[j] = i;
                        }
                    }
                    else
                    {
                        if (distanceTop[j - 1] > distance[i] && distance[i] >= distanceTop[j])
                        {
                            distanceTop[j] = distance[i];
                            distanceTopN[j] = i;
                        }
                    }
                }
            }
            print(distanceTop[0]);
            for (int i = 0; i < leave; i++)
            {
                copy2Local(GAarrayCopy, GAarray, distanceTopN[i], i, step * angleNumber); //一番優秀な遺伝子はGAarray[0]に残す

            }
            for (int i = leave; i < creatureNumber; i++)  //distanceの値に応じて次世代に残る個体を確率的に決める
            {
                a = r.Next(0, 100);
                for (int j = 0; j < creatureNumber; j++)
                {
                    if (j == 0)
                    {
                        if (0 <= a && a <= distanceRate[j])
                        {
                            //GAarrayCopyのj番目をGAarrayのi番目に転写
                            copy2Local(GAarrayCopy, GAarray, j, i, step * angleNumber);
                        }
                    }
                    else
                    {
                        if (distanceRate[j - 1] < a && a <= distanceRate[j])
                        {
                            copy2Local(GAarrayCopy, GAarray, j, i, step * angleNumber);
                        }
                    }


                }
            }

        }
    }
    void copy1(float[] a, float[] b, int x) //一次元配列のコピー　aの情報をbに書き込み
    {
        for (int i = 0; i < x; i++)
        {
            b[i] = a[i];
        }
    }
    void copy2(float[,] a, float[,] b, int x, int y)　//二次元配列のコピー　aの情報をbに書き込み
    {
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                b[i, j] = a[i, j];
            }
        }
    }
    void copy2Local(float[,] a, float[,] b, int x, int y, int z)　//二次元配列のx番目だけをコピー　aの情報をbに書き込み
    {                       //aのx番目をbのy番目に転写

        for (int j = 0; j < z; j++)
        {
            b[y, j] = a[x, j];
        }

    }
    void output(int[,] a, int u)
    {
        for (int i = 0; i < 1; i++)
        {
            foreach (int element in a)
            {
                print(a);
                Debug.Log(element);
            }
        }
    }
    void destroy()
    {
        if (time >= limitTime && one == true)
        {
            for (int i = 0; i < creatureNumber; i++)
            {
                Destroy(creature[i], 0); //0秒後に消失
            }
            one = false;
            time = 0;
        }
    }
    void walk()
    {
        for (int j = 0; j < creatureNumber; j++)
        {
            for (int i = 0; i < angleNumber; i++)
            {

                //HingeJoint hinge = transform.GetChild(i).gameObject.GetComponent<HingeJoint>();
                HingeJoint hinge = creature[j].transform.GetChild(i).gameObject.GetComponent<HingeJoint>();
                JointSpring hingeSpring = hinge.spring;
                hingeSpring.spring = 10000000;
                hingeSpring.damper = 2000000;
                hingeSpring.targetPosition = angle[j, i];
                hinge.spring = hingeSpring;
                hinge.useSpring = true;
            }
        }
    }


    void initiate()     //モデル生成
    {
        if (one == false)
        {
            //GameObject[] creature = new GameObject[creatureNumber];
            for (int i = 0; i < creatureNumber; i++)
            {
                creature[i] = Instantiate(prefab) as GameObject;
                creature[i].transform.position = new Vector3(10 + 25 * i, 4.6F, 0);
                creature[i].transform.rotation = Quaternion.Euler(94, 0.0f, 0.0f);

            }
            StartCoroutine("wait");
            one = true;

        }
    }
    IEnumerator wait()
    {
        yield return new WaitForSeconds(2F);
    }
    void memo()
    {
        //GameObject go = Instantiate(prefab) as GameObject;
        //GameObject go;

        //go.GetComponentsInChildren;
        //prefab=transform.FindChild("rightback").gameObject;
        //GameObject prefab = GameObject.Find("rightback2");
        //print(prefab.name);

        //HingeJoint hinge = transform.GetChild(1).gameObject.GetComponent<HingeJoint>();
        //HingeJoint hinge1 = transform.GetChild(0).gameObject.GetComponent<HingeJoint>();
        //HingeJoint hinge = prefab.GetComponent<HingeJoint>();
        //HingeJoint[] hinge = GetComponents<HingeJoint>();
        /*foreach (var col in hinge)
        {

            Debug.Log(col.name);
            JointSpring hingeSpring = col.spring;
            hingeSpring.spring = 25;
            hingeSpring.damper = 6;
            hingeSpring.targetPosition =k ;
            col.spring = hingeSpring;
            col.useSpring = true;
            k += 3;
        }*/
    }
}

