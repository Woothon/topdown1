using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManagerWolf : MonoBehaviour {

    [System.Serializable]
    public class AnimationType1
    {
        public AnimationClip Animation;
        public float speedAnimation = 1.0f;
    }

    [System.Serializable]
    public class AnimationType2
    {
        public AnimationClip Animation;
        public float speedAnimation = 1.0f;
        public bool speedTuning;
    }

    [System.Serializable]
    public class NormalAttackAnimation
    {
        public string attackName;
        public AnimationClip animation;
        public float speedAnimation = 1.0f;
        public float attackTimer = 0.5f;
        public float multipleDamage = 1f;
        public float flinchValue;
        public bool speedTuning;

        public GameObject attackEffect;
        public AudioClip soundEffect;
    }

    [System.Serializable]
    public class CriticalAttackAnimation
    {
        public string attackName;
        public AnimationClip animation;
        public float speedAnimation = 1.0f;
        public float attackTimer = 0.5f;
        public float multipleDamage = 1f;
        public float flinchValue;
        public bool speedTuning;

        public GameObject attackEffect;
        public AudioClip soundEffect;
    }

    [System.Serializable]
    public class TakeDamage
    {
        public string takeAttackName;
        public AnimationClip animation;
        public float speedAnimation = 1.0f;
    }

    public AnimationType1 idle, death;
    public AnimationType2 move;
    public List<NormalAttackAnimation> normalAttackAnimation;
    public List<CriticalAttackAnimation> criticalAttackAnimation;
    public List<TakeDamage> takeDamage;



    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
