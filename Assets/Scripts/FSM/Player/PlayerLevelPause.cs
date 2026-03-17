using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent (typeof(Player))]
public class PlayerLevelPause:MonoBehaviour
{
    protected Player m_player;
    protected LevelPauser m_pauser;

    protected virtual void Start()
    {
        m_player = GetComponent<Player>();
        m_pauser = LevelPauser.Instance;
    }

    protected virtual void Update()
    {
        if(m_player.inputs.GetPauseDown())
        {
            var value = m_pauser.paused;
            m_pauser.Pause(!value);
        }
    }
}

