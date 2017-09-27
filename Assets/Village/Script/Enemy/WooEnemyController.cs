using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnimationManagerWolf))]
[RequireComponent(typeof(WolfStatus))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(WooDropItem))]
public class WooEnemyController : MonoBehaviour {

    public enum ControlAnimationState { Idle, Move, WaitAttack, Attack, TakeAtk, Death };
    public enum EnemyBehaviour { Standing, MoveAround }
    public enum EnemyNature { Natural, Wild }
    public enum MoveAroundBehavior { MoveToNext, Waiting }

    public EnemyBehaviour behaviour;
    public EnemyNature nature;
    public MoveAroundBehavior moveBehavior;

    public GameObject target;
    public bool chaseTarget;
    public List<GameObject> modelMesh = new List<GameObject>();

    public Color colorTakeDamage;
    public float deadTimer;
    public bool deadTransparent;
    public float speedFade;
    public bool regenHP;
    public bool regenMP;

    public ControlAnimationState ctrlAnimState;

    public float movePhase;
    public float returnPhase;
    public float delayNextTargetMin;
    public float delayNextTargetMax;

    private AnimationManagerWolf animationManager;
    private WolfStatus enemyStatus;
    private Quest_Data questData;
    private CharacterController controller;

    private float defaultReturnPhase;
    private Vector3 spawnPoint;
    private float timeToWaitBeforeNextMove;
    private float delayAttack = 100;
    private Vector3 destinationPosition;
    private float destinationDistance;
    private Vector3 moveDir;
    private float moveSpeed;
    private Vector3 ctargetPos;
    private Vector3 targetPos;
    private Quaternion targetRotation;
    private bool checkCritical;
    private float flinchValue = 100;
    private Vector3 randomMoveVector;
    private float randomAngle;
    private Shader alphaShader;
    private Color[] defaultColor;
    private bool enableRegen;
    private GameObject detectArea;

    private float fadeValue;
    [HideInInspector]
    public float defaultHP, defaultMP;
    [HideInInspector]
    public int typeAttack;
    [HideInInspector]
    public int typeTakeAttack;
    [HideInInspector]
    public bool startFade;

    [HideInInspector]
    public int sizeMesh;
    // Use this for initialization
    void Start () {
        //set spawn point
        destinationPosition = this.transform.position;

        //get other component
        animationManager = this.GetComponent<AnimationManagerWolf>();
        enemyStatus = this.GetComponent<WolfStatus>();
        controller = this.GetComponent<CharacterController>();

        delayAttack = 100;
        flinchValue = 100;
        fadeValue = 1;

        spawnPoint = transform.position;

        defaultReturnPhase = returnPhase;
        defaultHP = enemyStatus.status.hp;
        defaultMP = enemyStatus.status.mp;

        defaultColor = new Color[modelMesh.Count];
        
        if(behaviour == EnemyBehaviour.MoveAround)
        {
            moveBehavior = MoveAroundBehavior.Waiting;
        }

        if(nature == EnemyNature.Wild)
        {
            detectArea = GameObject.Find("DetectArea");

            if(detectArea == null)
            {
                Debug.LogWarning("Can't found DetectArea in Enemy -" + enemyStatus.name);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        EnemyAnimationState();

        if(ctrlAnimState != ControlAnimationState.Death)
        {
            if(behaviour == EnemyBehaviour.MoveAround && !chaseTarget && !target)
            {
                if(moveBehavior == MoveAroundBehavior.MoveToNext)
                {
                    UpdateMovingToNextPatrolPoint();
                }else if(moveBehavior == MoveAroundBehavior.Waiting)
                {
                    UpdateWaitingForNextMove();
                }
            }
            else
            {
                EnemyMovementBattle();
            }
        }
        
    }

    //State Enemy
    void EnemyAnimationState()
    {
        if(ctrlAnimState == ControlAnimationState.Idle)
        {
            animationManager.animationState = animationManager.Idle;
        }

        if(ctrlAnimState == ControlAnimationState.Move)
        {
            animationManager.animationState = animationManager.Move;
        }

        if(ctrlAnimState == ControlAnimationState.WaitAttack)
        {
            animationManager.animationState = animationManager.Idle;
            WaitAttack();
        }

        if (ctrlAnimState == ControlAnimationState.Attack)
        {
            LookAtTarget(target.transform.position);

            if (checkCritical)
            {
                animationManager.animationState = animationManager.CriticalAttack;
                delayAttack = 100;
            }
            else
            {
                animationManager.animationState = animationManager.Attack;
                delayAttack = 100;
            }
        }

        if(ctrlAnimState == ControlAnimationState.TakeAtk)
        {
            animationManager.animationState = animationManager.TakeAttack;
        }

        if(ctrlAnimState == ControlAnimationState.Death)
        {
            if(this.gameObject.tag == "Enemy")
            {
                this.gameObject.tag = "Untagged";
                this.GetComponent<DropItem>().UseDropItem();
            }

            animationManager.animationState = animationManager.Death;

            if (startFade)
            {
                DeadTransparentAlpha(speedFade);
            }
        }
    }

    void RandomPostion()
    {
        randomAngle = Random.Range(0f, 91);
        randomMoveVector.x = Mathf.Sin(randomAngle) * movePhase + spawnPoint.x;
        randomMoveVector.z = Mathf.Cos(randomAngle) * movePhase + spawnPoint.z;
        randomMoveVector.y = transform.position.y;

        targetRotation = Quaternion.LookRotation(randomMoveVector - transform.position);
        destinationPosition = randomMoveVector;
    }

    void UpdateMovingToNextPatrolPoint()
    {
        destinationDistance = Vector3.Distance(destinationPosition, this.transform.position);

        if(destinationDistance < 1f)
        {
            timeToWaitBeforeNextMove = Random.Range(delayNextTargetMin, delayNextTargetMax);
            moveBehavior = MoveAroundBehavior.Waiting;
            ctrlAnimState = ControlAnimationState.Idle;
            moveSpeed = 0;
        }

        if(ctrlAnimState == ControlAnimationState.Move)
        {
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetRotation, Time.deltaTime * 25);

            if (controller.isGrounded)
            {
                moveDir = Vector3.zero;
                moveDir = transform.TransformDirection(Vector3.forward* moveSpeed);
            }
        }
        else
        {
            moveDir = Vector3.Lerp(moveDir, Vector3.zero, Time.deltaTime * 10);

        }

        moveDir.y -= 20 * Time.deltaTime;
        controller.Move(moveDir* Time.deltaTime);
    }

    public void DeadTransparentSetup()
    {
        int index = 0;

        alphaShader = Shader.Find("Transparent/Diffuse");

        while (index < modelMesh.Count)
        {
            modelMesh[index].GetComponent<Renderer>().material.shader = alphaShader;
            index++;
        }

        startFade = true;
        deadTransparent = false;
    }

    //wait for next move
    void UpdateWaitingForNextMove()
    {
        timeToWaitBeforeNextMove -= Time.deltaTime;
        if(timeToWaitBeforeNextMove < 0.0f)
        {
            RandomPostion();
            moveBehavior = MoveAroundBehavior.MoveToNext;
            ctrlAnimState = ControlAnimationState.Move;
            moveSpeed = enemyStatus.status.moveSpd;
        }
    }

    void LookAtTarget(Vector3 _targetPos)
    {
        targetPos.x = _targetPos.x;
        targetPos.y = this.transform.position.y;
        targetPos.z = _targetPos.z;
        this.transform.LookAt(targetPos);
    }

    //Enemy movement when enemy found target
    void EnemyMovementBattle()
    {
        if(chaseTarget == true)
        {
            LookAtTarget(target.transform.position);
            destinationDistance = Vector3.Distance(target.transform.position, this.transform.position);//Check Distance Enemy to Hero

            if(destinationDistance <= enemyStatus.status.atkRange)
            {
                if(ctrlAnimState == ControlAnimationState.Move || ctrlAnimState == ControlAnimationState.Idle)
                {
                    ctrlAnimState = ControlAnimationState.WaitAttack;
                }

                moveSpeed = 0;
            }else if(destinationDistance > enemyStatus.status.atkRange){
                if(ctrlAnimState == ControlAnimationState.Move || ctrlAnimState == ControlAnimationState.Idle || ctrlAnimState == ControlAnimationState.WaitAttack)
                {
                    ctrlAnimState = ControlAnimationState.Move;
                }
                moveSpeed = enemyStatus.status.moveSpd;
            }
        }else if(chaseTarget == false)
        {
            LookAtTarget(spawnPoint);
            destinationDistance = Vector3.Distance(spawnPoint, this.transform.position);

            if(destinationDistance <= enemyStatus.status.atkRange)
            {
                if (ctrlAnimState == ControlAnimationState.Move || ctrlAnimState == ControlAnimationState.Idle)
                {
                    ctrlAnimState = ControlAnimationState.Idle;
                }

                moveSpeed = 0;

                returnPhase = defaultReturnPhase;

                if(enableRegen)
                {
                    if (regenHP)
                        enemyStatus.status.hp = Mathf.FloorToInt(defaultHP);
                    if (regenMP)
                        enemyStatus.status.mp = Mathf.FloorToInt(defaultMP);

                    destinationPosition = this.transform.position;
                    target = null; 
                }

            }
            else
            {
                if(ctrlAnimState == ControlAnimationState.Move || ctrlAnimState == ControlAnimationState.Idle || ctrlAnimState == ControlAnimationState.WaitAttack)
                {
                    ctrlAnimState = ControlAnimationState.Move;
                    
                }

                moveSpeed = enemyStatus.status.moveSpd;
            }
        }

        //Check distance Spawn
        float distanceSpawn = Vector3.Distance(spawnPoint, this.transform.position);
        if(distanceSpawn > returnPhase)
        {
            if (regenHP || regenMP)
                enableRegen = true;

            chaseTarget = false;

            returnPhase += 3;
        }

        // Move to distination;
        if(ctrlAnimState == ControlAnimationState.Move)
        {
            if (controller.isGrounded)
            {
                moveDir = Vector3.zero;
                moveDir = transform.TransformDirection(Vector3.forward * moveSpeed);
            }
        }
        else
        {
            moveDir = Vector3.Lerp(moveDir, Vector3.zero, Time.deltaTime * 10);
        }

        moveDir.y -= 20 * Time.deltaTime;
        controller.Move(moveDir * Time.deltaTime);
    }

    void WaitAttack()
    {
        PlayerStatus playerStatus;
        playerStatus = target.GetComponent<PlayerStatus>();
        if(playerStatus.statusCal.hp <= 0)
        {
            ResetState();
        }

        if(delayAttack > 0)
        {
            delayAttack -= Time.deltaTime * enemyStatus.status.atkSpd;
        }else if(delayAttack <= 0)
        {
            checkCritical = CriticalCal(enemyStatus.status.criticalRate);

            if (checkCritical)
            {
                typeAttack = Random.Range(0, animationManager.criticalAttack.Count);
                animationManager.checkAttack = false;
            }
            else
            {
                typeAttack = Random.Range(0, animationManager.normalAttack.Count);
                animationManager.checkAttack = false;
            }

            ctrlAnimState = ControlAnimationState.Attack;
        }
    }

    bool CriticalCal(float criticalStat)
    {
        float calCritical = criticalStat - Random.Range(0, 101f);

        if(calCritical > 0)
        {
            return true;
        }

         return false;

    }

    public void ResetState()
    {
        target = null;
        chaseTarget = false;
        destinationPosition = this.transform.position;
        ctrlAnimState = ControlAnimationState.Idle;
    }

    public void DeadTransparentAlpha(float speedFade)
    {
        int index = 0;
        Color[] colorDef = new Color[modelMesh.Count];

        while(index < modelMesh.Count)
        {
            colorDef[index] = modelMesh[index].GetComponent<Renderer>().material.color;
            Color alphaColor = new Color(modelMesh[index].GetComponent<Renderer>().material.color.r, modelMesh[index].GetComponent<Renderer>().material.color.g, modelMesh[index].GetComponent<Renderer>().material.color.b, fadeValue);
            modelMesh[index].gameObject.GetComponent<Renderer>().material.color = alphaColor;

            if(modelMesh[index].gameObject.GetComponent<Renderer>().material.color.a > 0)
            {
                if (fadeValue > 0)
                {
                    fadeValue -= Time.deltaTime * speedFade;
                }
                else
                {
                    fadeValue = 0;
                }
                    
            }

            index++;
        }
    }
}
