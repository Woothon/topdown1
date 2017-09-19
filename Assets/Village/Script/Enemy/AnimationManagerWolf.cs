using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManagerWolf : MonoBehaviour {
    public delegate void AnimationHandle();
    public AnimationHandle animationState;

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
    public class AnimationTakeAttack
    {
        public string takeAttackName;
        public AnimationClip animation;
        public float speedAnimation = 1.0f;
    }

    public AnimationType1 idle, death;
    public AnimationType2 move;
    public List<NormalAttackAnimation> normalAttack;
    public List<CriticalAttackAnimation> criticalAttack;
    public List<AnimationTakeAttack> takeAttack;

    private WooEnemyController enemyController;
    private WolfStatus enemyStatus;
    [HideInInspector]
    public bool checkAttack;

    // Use this for initialization
    void Start () {
        enemyController = GetComponent<WooEnemyController>();
        enemyStatus = GetComponent<WolfStatus>();
	}
	
	// Update is called once per frame
	void Update () {
		if(animationState != null){
            animationState();
        }
	}

    public void Idle()
    {
        GetComponent<Animation>().CrossFade(idle.Animation.name);
        GetComponent<Animation>()[idle.Animation.name].speed = idle.speedAnimation;
    }

    public void Move()
    {
        GetComponent<Animation>().Play(move.Animation.name);

        if (move.speedTuning)
        {
            GetComponent<Animation>()[move.Animation.name].speed = (enemyStatus.status.moveSpd / 3f) / move.speedAnimation;
        }
        else
        {
            GetComponent<Animation>()[move.Animation.name].speed = move.speedAnimation;
        }
    }

    public void Attack()
    {
        GetComponent<Animation>().Play(normalAttack[enemyController.typeAttack].animation.name);

        if (normalAttack[enemyController.typeAttack].speedTuning)  //Enable Speed Tuning
        {
            GetComponent<Animation>()[normalAttack[enemyController.typeAttack].animation.name].speed = (enemyStatus.status.atkSpd / 100f) / normalAttack[enemyController.typeAttack].speedAnimation;
        }
        else
        {
            GetComponent<Animation>()[normalAttack[enemyController.typeAttack].animation.name].speed = normalAttack[enemyController.typeAttack].speedAnimation;
        }

        //Calculate Attack
        if (GetComponent<Animation>()[normalAttack[enemyController.typeAttack].animation.name].normalizedTime > normalAttack[enemyController.typeAttack].attackTimer && !checkAttack)
        {
            //Attack Damage
            HeroController enemy;
            enemy = enemyController.target.GetComponent<HeroController>();
            enemy.GetDamage(enemyStatus.status.atk * normalAttack[enemyController.typeAttack].multipleDamage, enemyStatus.status.hit, normalAttack[enemyController.typeAttack].flinchValue
                            , normalAttack[enemyController.typeAttack].attackEffect, normalAttack[enemyController.typeAttack].soundEffect);
            checkAttack = true;
        }

        if (GetComponent<Animation>()[normalAttack[enemyController.typeAttack].animation.name].normalizedTime > 0.9f)
        {
            enemyController.ctrlAnimState = WooEnemyController.ControlAnimationState.WaitAttack;
            checkAttack = false;
        }
    }

    public void CriticalAttack()
    {
        GetComponent<Animation>().Play(criticalAttack[enemyController.typeAttack].animation.name);

        if (criticalAttack[enemyController.typeAttack].speedTuning)  //Enable Speed Tuning
        {
            GetComponent<Animation>()[criticalAttack[enemyController.typeAttack].animation.name].speed = (enemyStatus.status.atkSpd / 100f) / criticalAttack[enemyController.typeAttack].speedAnimation;
        }
        else
        {
            GetComponent<Animation>()[criticalAttack[enemyController.typeAttack].animation.name].speed = criticalAttack[enemyController.typeAttack].speedAnimation;
        }

        //Calculate Attack
        if (GetComponent<Animation>()[criticalAttack[enemyController.typeAttack].animation.name].normalizedTime > criticalAttack[enemyController.typeAttack].attackTimer && !checkAttack)
        {
            //Attack Damage
            HeroController enemy;
            enemy = enemyController.target.GetComponent<HeroController>();

            enemy.GetDamage(enemyStatus.status.atk * criticalAttack[enemyController.typeAttack].multipleDamage, 10000, criticalAttack[enemyController.typeAttack].flinchValue
                            , criticalAttack[enemyController.typeAttack].attackEffect, criticalAttack[enemyController.typeAttack].soundEffect);

            checkAttack = true;
        }

        if (GetComponent<Animation>()[criticalAttack[enemyController.typeAttack].animation.name].normalizedTime > 0.9f)
        {
            enemyController.ctrlAnimState = WooEnemyController.ControlAnimationState.WaitAttack;
            checkAttack = false;
        }
    }

    public void TakeAttack()
    {
        GetComponent<Animation>().CrossFade(takeAttack[enemyController.typeTakeAttack].animation.name);
        GetComponent<Animation>()[takeAttack[enemyController.typeTakeAttack].animation.name].speed = takeAttack[enemyController.typeTakeAttack].speedAnimation;

        if (GetComponent<Animation>()[takeAttack[enemyController.typeTakeAttack].animation.name].normalizedTime > 0.9f)
        {
            enemyController.ctrlAnimState = WooEnemyController.ControlAnimationState.WaitAttack;
        }
    }

    public void Death()
    {
        GetComponent<Animation>().CrossFade(death.Animation.name);
        GetComponent<Animation>()[death.Animation.name].speed = death.speedAnimation;

        if (GetComponent<Animation>()[death.Animation.name].normalizedTime > 0.9f)
        {
            if (enemyController.deadTransparent)
            {
                enemyController.DeadTransparentSetup();
            }
        }
    }
}
