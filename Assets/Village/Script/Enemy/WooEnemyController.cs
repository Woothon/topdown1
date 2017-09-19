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
    private float deplayAttack = 100;
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

        deplayAttack = 100;
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
                    UpdateWaitingforNextMove();
                }
            }
        }
        else
        {
            EnemyMovementBattle();
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
                deplayAttack = 100;
            }
            else
            {
                animationManager.animationState = animationManager.Attack;
                deplayAttack = 100;
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
}
