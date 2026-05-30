using UnityEngine;

public class IdlePlayerState : PlayerState
{
    protected override void OnEnter(Player entity)
    {
    }

    protected override void OnExit(Player entity)
    {
    }

    protected override void OnStep(Player entity)
    {
        Debug.Log("IdleStep");
    }

    public override void OnContact(Player entity, Collider other)
    {
    }
}