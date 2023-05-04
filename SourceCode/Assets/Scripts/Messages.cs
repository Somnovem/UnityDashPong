using UnityEngine;
using Mirror;
public struct SynchronizingStatsMessage : NetworkMessage
{
    public int goalstate;
    public float ballSpeed;
    public SynchronizingStatsMessage(int goalstate,float ballSpeed)
    {
        this.goalstate = goalstate;
        this.ballSpeed = ballSpeed;
    }
}

public struct StartGameMessage : NetworkMessage
{
    public bool startGame;
}
