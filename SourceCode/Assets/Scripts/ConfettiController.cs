using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfettiController : MonoBehaviour
{
    public static ConfettiController instance;
    public ParticleSystem ps;
    private void Start()
    {
        instance = this;
    }
    public void SprayConfetti()
    {
        ps.Play();
        StartCoroutine(StopSpraying());
    }

    private IEnumerator StopSpraying()
    {
        yield return new WaitForSeconds(1f);
        ps.Stop();
    }
}
