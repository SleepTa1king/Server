using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class IdleEnemyState : EnemyState
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
        enemy.Friction(deltaTime);
    }

    public override void OnContact(Enemy enemy, Collider other)
    {
    }
}
