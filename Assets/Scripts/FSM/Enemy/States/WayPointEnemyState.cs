using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WayPointEnemyState : EnemyState
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

        var destination = enemy.waypoints.current.position;
        destination = new Vector3(destination.x, enemy.position.y, destination.z);

        var head = destination - enemy.position;
        var distance = head.magnitude;
        var direction = head / distance;

        if (distance <= enemy.stats.current.waypointMinDistance)
        {
            enemy.waypoints.Next();
        }
        else
        {
            enemy.Accelerate(direction, enemy.stats.current.waypointAcceleration, enemy.stats.current.waypointTopSpeed, deltaTime);
            if (enemy.stats.current.faceWaypoint)
            {
                enemy.FaceDirectionSmooth(direction, deltaTime);
            }
        }
    }

    public override void OnContact(Enemy enemy, Collider other)
    {
    }
}
