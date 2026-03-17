using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerController:MonoBehaviour
{
    public void AddHealth(Player player, int amount) => player.health.Increase(amount);
    public void AddHealt(Player player) => AddHealth(player, 1);
}

