using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class IdlePlayerState:PlayerState
{
    protected override void OnEnter(Player player)
    {
        
    }

    protected override void OnStep(Player player, float deltaTime)
    {
        player.Gravity(deltaTime);
        player.SnapToGround(deltaTime);
        player.Jump();
        player.Fall();
        player.Spin();
        player.PickAndThrow();

        var inputDirection = player.inputs.GetMovementDirection();

        if(inputDirection.sqrMagnitude > 0 || player.lateralVelocity.sqrMagnitude > 0)
        {
            player.states.Change<WalkPlayerState>();
            //Debug.Log("inputDirection"+inputDirection);
        }
        else if(player.inputs.GetCrouchAndCraw())
        {
            player.states.Change<CrouchPlayerState>();
        }
    }

    protected override void OnExit(Player player)
    {

    }

    public override void OnContact(Player player,Collider other)
    {

    }
}

