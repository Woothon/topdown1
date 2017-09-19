using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewSpawner : MonoBehaviour {

    public int monsterLimit;
    public GameObject[] monters;
    private Object[] monterList;
    public float spawnTime;

    private int currentMonster = 0;
    public bool showArea;

    public Color areaColor;
    public float areaRadius;
	// Use this for initialization
	void Start () {
        monterList = new Object[monsterLimit];
        //定时调用产生怪物方法
        InvokeRepeating("SpawnMonster", spawnTime, spawnTime);
	}
	
	// Update is called once per frame
	void Update () {
		if(currentMonster >= monsterLimit)
        {
            //取消调用产生怪物方法
            CancelInvoke("SpawnMonster");
            RegenerateMonster();
        }
	}

    //产生怪物
    private void SpawnMonster()
    {
        //遍历怪物列表，遇到首个怪物为空的则建一个怪物然后返回。
        for (int i = 0; i < monsterLimit; i++)
        {
            if (monterList[i] == null)
            {
                //在怪物产生点随机位置以默认转向生成怪物库中随机的一个。
                monterList[i] = GameObject.Instantiate(monters[Random.Range(0,monters.Length)], RandomLocation(), Quaternion.identity);
                currentMonster++;
                
                break;
            }
           
        }
    }

    //重新生成怪物
    private void RegenerateMonster()
    {
        //遍历怪物列表，如果那个怪物为空表示被销毁了。那么隔一个产生间隔重建该怪物
        for(int i = 0; i < monsterLimit; i++)
        {
            if (monterList[i] == null)
            {
                Invoke("SpawnMonster", spawnTime);
                currentMonster--;
            }
        }
    }

  
    //定位一个随机的位置
    private Vector3 RandomLocation()
    {
        //取一个随机弧度
        float radian = Random.Range(0f, 2*Mathf.PI);
        //以随机弧度值来获取以生产地半径画圆的任一点作为随机位置
        return new Vector3(transform.position.x + Mathf.Sin(radian)*areaRadius, transform.position.y,transform.position.z + Mathf.Cos(radian)*areaRadius);
    }

    //当本Gameobject被选中时执行该方法。
    void OnDrawGizmosSelected()
    {
        if (!showArea)
        {
            //注意需要透明度。可以考虑调成半透明。如果没有透明度，色球是不可见的。
            Gizmos.color = areaColor;
            Gizmos.DrawSphere(transform.position, areaRadius);
        }

    }

    //当本Gameobject没有被选中时执行该方法。
    void OnDrawGizmos()
    {
        if (showArea)
        {
            //这是创建一个有颜色的球体。
            Gizmos.color = new Color(0.0f, 0.5f, 0.5f, 0.3f); 
            Gizmos.DrawSphere(transform.position, areaRadius);
        }
    }

   
}
