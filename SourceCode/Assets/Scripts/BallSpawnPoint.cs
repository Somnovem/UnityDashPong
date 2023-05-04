using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawnPoint : MonoBehaviour
{
    void Start() => PlayerSpawnSystem.ballSpawnPoint = transform;
}
