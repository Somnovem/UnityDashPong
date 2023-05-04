using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class BallMovement : NetworkBehaviour
{
    private float pullForce = 11.0f;
    private float pullInterval = 0.5f;

    public GameObject WallHitParticle;
    private TrailRenderer trail;
    public Rigidbody2D rb;
    public static bool pointAlreadyScored;

    private Vector3 startingPosition;

    public override void OnStartServer()
    {
        base.OnStartServer();
        rb.simulated = true;
        StartCoroutine(AwaitStart());
        rb.isKinematic = true;
        StartCoroutine(UnlockMovement());
    }

    private void Start()
    {   
        InvokeRepeating("PullTowardsCenter", 0f, pullInterval);
        startingPosition = rb.transform.position;
        trail = GetComponent<TrailRenderer>();
    }

    private void PullTowardsCenter()
    {
        Vector3 direction = -transform.position.normalized;
        GetComponent<Rigidbody2D>().AddForce(direction * pullForce, ForceMode2D.Force);
    }

    [ServerCallback]
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Wall"))
        {   
            GameObject particle = Instantiate(WallHitParticle, col.GetContact(0).point, Quaternion.identity);
            NetworkServer.Spawn(particle);
        }
        if (col.transform.GetComponent<NetworkGamePlayer>())
        {
            float y = HitFactor(transform.position,
                                col.transform.position,
                                col.collider.bounds.size.y);
            float x = col.relativeVelocity.x > 0 ? 1 : -1;
            Vector2 dir = new Vector2(x, y).normalized;
            rb.AddForce(dir * 5f,ForceMode2D.Impulse);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(!pointAlreadyScored)
        {
            pointAlreadyScored = true;
            trail.enabled = false;
            if(isServer)
            {
            SynchronizingStatsMessage msg = new SynchronizingStatsMessage(col.CompareTag("MyPad") ? -1 : 1,rb.velocity.magnitude);
            NetworkServer.SendToReady(msg);
            StartCoroutine(EnableTrailAndRestartStats());
            StartCoroutine(RestartRound());
            DestroyPads();
            }
        }

    }

    float HitFactor(Vector2 ballPos, Vector2 racketPos, float racketHeight)
    {
        return (ballPos.y - racketPos.y) / racketHeight;
    }

    [Server]
    private void DestroyPads()
    {
        GameObject[] copies = GameObject.FindGameObjectsWithTag("DashPad");
        for(int i = 0;i < copies.Length;++i)
        {
            try
            {
                NetworkServer.Destroy(copies[i]);
            }
            catch{}
        }
    }

    #region Coroutines
    private IEnumerator RestartRound()
    {
        yield return new WaitForSeconds(3.0f);
        rb.velocity = Vector2.zero;
        rb.transform.position = startingPosition;
        pointAlreadyScored = false;
    }

    private IEnumerator EnableTrailAndRestartStats()
    {
        yield return new WaitForSeconds(0.5f);
        trail.enabled = true;
        SynchronizingStatsMessage msg = new SynchronizingStatsMessage(0,0f);
        NetworkServer.SendToReady(msg);
    }
    
    private IEnumerator AwaitStart()
    {
        yield return new WaitForSeconds(0.5f);
        StartGameMessage msg = new StartGameMessage();
        msg.startGame = true;
        NetworkServer.SendToReady(msg);
    }
    
    private IEnumerator UnlockMovement()
    {
        yield return new WaitForSeconds(3f);
        rb.isKinematic = false;
    }
    #endregion
}
