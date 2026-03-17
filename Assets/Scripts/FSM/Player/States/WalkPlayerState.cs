using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WalkPlayerState:PlayerState
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
        player.Dash();
        player.Spin();
        player.PickAndThrow();

        var inputDirection = player.inputs.GetMovementCameraDirection();
        //Debug.Log("inputDirection" + inputDirection);

        if (inputDirection.sqrMagnitude > 0)
        {
            var dot = Vector3.Dot(inputDirection,player.lateralVelocity);

            if (dot >= player.stats.current.brakeTreshold)
            {
                //超过加速阈值 ->正常加速与转向
                player.Accelerate(inputDirection, deltaTime);
                player.FaceDirectionSmooth(inputDirection, deltaTime);
            }
            else
            {
                //低于刹车阈值->进入减速状态
                player.states.Change<BrakePlayerState>();
            }
        }
        else
        {
            //摩擦力进行减速
            player.Friction(deltaTime);

            if (player.lateralVelocity.sqrMagnitude <= 0)
            {
                player.states.Change<IdlePlayerState>();

            }
        }
        if (player.inputs.GetCrouchAndCraw())
        {
            player.states.Change<CrouchPlayerState>();
        }


    }

    protected override void OnExit(Player player)
    {

    }

    public override void OnContact(Player player, Collider other)
    {
        player.PushRigidbody(other);
    }
}

