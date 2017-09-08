using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfStatus : MonoBehaviour {

	public string enemyName;
    public int enemyId;
    public Attribute status;
    public int expGive;

    [System.Serializable]
    public class Attribute
    {
        public int hp, mp, atk, def, spd, hit;
        public float criticalRate, atkSpd, atkRange, moveSpd;
    }
}
