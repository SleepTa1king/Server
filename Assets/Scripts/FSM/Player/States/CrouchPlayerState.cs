using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class CrouchPlayerState:PlayerState
{
    protected override void OnEnter(Player player)
    {
        player.ResizeColider(player.stats.current.crouchHeight);
    }

    protected override void OnStep(Player player, float deltaTime)
    {
        player.Gravity(deltaTime);
        player.SnapToGround(deltaTime);
        //player.Jump();
        player.Fall();
        player.Decelerate(player.stats.current.crouchFriction, deltaTime);

        var inputDirection = player.inputs.GetMovementDirection();
        if (player.inputs.GetCrouchAndCraw() || !player.canStandUp)
        {
            if(inputDirection.sqrMagnitude >0 &&!player.holding)
            {
                if(player.lateralVelocity.sqrMagnitude == 0)
                {
                    player.states.Change<CrawlingPlayerState>();
                }
            }
            else if(player.inputs.GetJumpDown())
            {
                player.Backflip(player.stats.current.backflipBackwardForce);
            }
        }
        else
        {
            
            player.states.Change<IdlePlayerState>();
        }
    }

    protected override void OnExit(Player player)
    {
        player.ResizeColider(player.originHeight);
    }

    public override void OnContact(Player player, Collider other)
    {

    }
}

