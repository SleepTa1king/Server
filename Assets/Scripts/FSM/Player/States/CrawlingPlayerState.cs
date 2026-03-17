using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CrawlingPlayerState:PlayerState
{
    protected override void OnEnter(Player player)
    {
        player.ResizeColider(player.stats.current.crouchHeight);
    }

    protected override void OnStep(Player player, float deltaTime)
    {
        player.Gravity(deltaTime);
        player.SnapToGround(deltaTime);
        player.Jump();
        player.Fall();

        var inputDirection = player.inputs.GetMovementCameraDirection();
        if (player.inputs.GetCrouchAndCraw() || !player.canStandUp)
        {
            if(inputDirection.sqrMagnitude >0)
            {
                player.CrawlingAccelerate(inputDirection, deltaTime);

                player.FaceDirectionSmooth(player.lateralVelocity, deltaTime);
            }
            else
            {
                player.Decelerate(player.stats.current.crawlingFriction, deltaTime);
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
        player.WallDrag(other);
    }
}

