using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class NetworkGamePlayer : NetworkBehaviour
{
    #region Calculation variables
    [SerializeField] private float movementSpeed = 7f;
    [SerializeField] private int maxDashes = 3;
    [SerializeField] private float dashRestoreTime = 4.5f;

    [SerializeField] private float dashSpeedMultiplier = 1.5f;

    public static bool isInputAvaliable;
    private int avaliableDashes;
    private float dashTimer;
    private Vector3 startingPosition;
    private int bluePoints;
    private int redPoints;
    private int gamestate;
    #endregion

    #region Graphic variables
    public GameObject WallHitParticle;
    public GameObject PadPrefab; 

    private Rigidbody2D rb;
    private TrailRenderer trail;

    public delegate void PointScoredDelegate(int redPoints, int bluePoints,float ballSpeed,string winnerText);
    public event PointScoredDelegate PointScored;

    public delegate void GameStartingDelegate();
    public event GameStartingDelegate GameStarting;
    #endregion

    #region Rooms

    [SyncVar]
    private string displayName = "Loading...";

    private NetworkManagerPong room;
    private NetworkManagerPong Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerPong;
        }
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);
        Room.GamePlayers.Add(this);
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    public string GetDisplayName() => displayName;

    #endregion
    private static int currentGoalstate;
    private static float currentBallSpeed;

    private static bool ballWasSpawned;

    private bool registeredHandler;

    void Start()
    {
        if(isClient && !registeredHandler)
        {
            NetworkClient.RegisterHandler<SynchronizingStatsMessage>(OnSynchronizingStatsMessageReceived);
            NetworkClient.RegisterHandler<StartGameMessage>(OnStartGameMessageReceived);
            registeredHandler = true;
        }
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
        Color firstPlayerColor,secondPlayerColor;
        if(isServer)
        {
            firstPlayerColor = Color.red;
            secondPlayerColor = Color.blue;
        }
        else
        {
            firstPlayerColor = Color.blue;
            secondPlayerColor = Color.red;
        }
        Color myColor = isOwned ? firstPlayerColor : secondPlayerColor;
        gameObject.LeanColor(myColor,0f);
        SetTrailColor(myColor);   
        startingPosition = rb.transform.position;
        dashTimer = Time.time;
        isInputAvaliable = false;
        gamestate = 0;
    }

    private void SetTrailColor(Color color)
    {
        trail.startColor = color;
        trail.endColor = color;
    }

    void Update()
    {
        if(!isOwned)return;
        if(!ballWasSpawned)return;
        if(!isInputAvaliable)return;
        if(currentGoalstate != 0)
        {
            isInputAvaliable = false;
            if(currentGoalstate == 1) redPoints++;
            else bluePoints++;
            if(redPoints == 5)  gamestate =1;
            else if(bluePoints == 5)gamestate = -1;
            string res = "";
            if(gamestate != 0)res = gamestate == 1 ? "RED WON" : "BLUE WON";
            else StartCoroutine(Reset());
            PointScored?.Invoke(redPoints,bluePoints,currentBallSpeed,res);
        }
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");


        Vector2 movement = new Vector2(moveHorizontal * movementSpeed, moveVertical * movementSpeed);
        
        rb.AddForce(movement, ForceMode2D.Force);

        
        if (rb.velocity.magnitude > 0f)
        {
            Vector2 friction = -rb.velocity.normalized * Mathf.Sqrt(rb.velocity.magnitude) * 2f;
            rb.AddForce(friction, ForceMode2D.Force);
        }

        if(Time.time - dashTimer >= dashRestoreTime)
        {
            if(avaliableDashes < maxDashes)
            {
                avaliableDashes++;
            }
            dashTimer = Time.time;
        }

        if(Input.GetKeyDown(KeyCode.LeftShift) && avaliableDashes > 0)
        {
            Vector3 padLocation = rb.transform.position;
            rb.AddForce(movement * dashSpeedMultiplier, ForceMode2D.Impulse);
            avaliableDashes--;
            padLocation.y += 0.2f;
            float angle = Mathf.Atan2(movement.y, movement.x)*Mathf.Rad2Deg - 90;
            if(isServer)
                SpawnPad(angle);
            else
                CmdSpawnPad(angle);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Instantiate(WallHitParticle, collision.GetContact(0).point, Quaternion.identity);
        }
    }
    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(3.0f);
        trail.enabled = false;
        StartCoroutine(EnableTrail());
        rb.velocity = Vector2.zero;
        rb.transform.position = startingPosition;
        avaliableDashes = 0;
        isInputAvaliable = true;
        dashTimer = Time.time;
    }

    public static void ResetStaticVariables()
    {
        currentGoalstate = 0;
        currentBallSpeed = 0f;
        ballWasSpawned = false;
    }

    public void OnSynchronizingStatsMessageReceived(SynchronizingStatsMessage message)
    {
        currentGoalstate = message.goalstate;
        currentBallSpeed = message.ballSpeed;
    }

    public void OnStartGameMessageReceived(StartGameMessage message)
    {
        ballWasSpawned = message.startGame;
        StartCoroutine(UnlockMovement());
        GameStarting?.Invoke();
        
    }

    [Server]
    public void SpawnPad(float angle)
    {
        GameObject pad = Instantiate(PadPrefab, transform.position, Quaternion.identity);
        var rigidbody = pad.GetComponent<Rigidbody2D>();
        rigidbody.transform.localScale = new Vector3(rigidbody.transform.localScale.x, 0, rigidbody.transform.localScale.z);
        rigidbody.isKinematic = true;
        rigidbody.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        NetworkServer.Spawn(pad); 
        pad.GetComponent<DashPad>().Init();
    }

    [Command]
    public void CmdSpawnPad(float angle)
    {
        SpawnPad(angle);
    }
    
    private IEnumerator EnableTrail()
    {
        yield return new WaitForSeconds(0.5f);
        trail.enabled = true;
    }

    private IEnumerator UnlockMovement()
    {
        yield return new WaitForSeconds(3.5f);
        isInputAvaliable = true;
    }
}
