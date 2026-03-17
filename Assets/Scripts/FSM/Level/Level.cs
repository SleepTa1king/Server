using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Level:Singleton<Level>
{
    protected Player m_player;
    public Player player
    {
        get
        {
            if (!m_player)
            {
                m_player = FindFirstObjectByType<Player>();
            }
            return m_player;
        }
    }
}

