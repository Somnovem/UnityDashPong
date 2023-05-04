using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class DashPad : NetworkBehaviour
{
    private bool initialized;

    private float lifeTime = 6f;
    private float lifeTimer;

    [Server]
    public void Init()
    {
        initialized = true;
    }

    void Start()
    {
        LeanTween.scaleY(gameObject, 3.36f, 0.3f)
                     .setEase(LeanTweenType.easeOutCirc);
        lifeTimer = Time.time;
        StartCoroutine(UnlockPositionAndRotation(gameObject.GetComponent<Rigidbody2D>()));
        StartCoroutine(ShrinkPad(gameObject));
    }

    void Update()
    {
        if (initialized && isServer)
        {
            if(Time.time - lifeTimer >= lifeTime)NetworkServer.Destroy(gameObject);
        }
    }

    private IEnumerator UnlockPositionAndRotation(Rigidbody2D rigidbody)
    {
        yield return new WaitForSeconds(0.5f);
        try
        {
            rigidbody.isKinematic = false;
        }
        catch{}//Pad was destroyed
        
    }

    private IEnumerator ShrinkPad(GameObject pad)
    {
        yield return new WaitForSeconds(lifeTime-0.3f);
        try
        {
         LeanTween.scaleY(pad, 0f, 0.3f)
                  .setEase(LeanTweenType.easeInElastic);
        }
        catch{} //Pad was destroyed
    }
}
