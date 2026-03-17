using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
public class FollowEnemyState : EnemyState
{
    protected override void OnEnter(Enemy enemy)
    {
    }

    protected override void OnExit(Enemy enemy)
    {
       
    }

    protected override void OnStep(Enemy enemy, float deltaTime)
    {
        enemy.Gravity(deltaTime);
        enemy.SnapToGround(deltaTime);
        var head = enemy.player.position - enemy.position;
        var direction = new Vector3(head.x, 0, head.z).normalized;
        enemy.Accelerate(direction, enemy.stats.current.turningDrag, 
            enemy.stats.current.followAcceleration, enemy.stats.current.followTopSpeed, deltaTime);
        enemy.FaceDirectionSmooth(direction, deltaTime);

    }
    public override void OnContact(Enemy enemy, Collider other)
    {
    }

}

