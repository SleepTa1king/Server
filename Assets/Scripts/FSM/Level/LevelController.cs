using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class LevelController :MonoBehaviour
{
    protected LevelPauser  m_pauser=>LevelPauser.Instance;
    protected LevelRespawner m_respawnser => LevelRespawner.Instance;
    protected LevelScore m_score => LevelScore.Instance;
    public virtual void AddCoins(int amount)
    {
        m_score.coins += amount;
    }

    public virtual void Respawn(bool consumeRetries) => m_respawnser.Respawn(consumeRetries);
    public virtual void Pause(bool value) => m_pauser.Pause(value); 
}

