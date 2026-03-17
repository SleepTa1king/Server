using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public class FallPlayerState:PlayerState
{
    protected override void OnEnter(Player player)
    {

    }

    protected override void OnStep(Player player, float deltaTime)
    {
        player.Gravity(deltaTime);
        player.FaceDirectionSmooth(player.lateralVelocity, deltaTime);
        player.AccelerateToInputDirection(deltaTime);
        player.Jump();
        player.Dash();
        player.StompAttack();
        player.Spin();
        player.LedgeGrab();
        player.AirDive();
        player.Glide();
        player.PickAndThrow();

        if (player.isGrounded)
        {
            player.states.Change<IdlePlayerState>();
        }
    }

    protected override void OnExit(Player player)
    {

    }
    public override void OnContact(Player player, Collider other)
    {
        //player.WallDrag(other);
        player.GrabPole(other);
        player.PushRigidbody(other);

    }
}

