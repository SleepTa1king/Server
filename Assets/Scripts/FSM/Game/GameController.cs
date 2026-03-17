using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameController:MonoBehaviour
{
    protected Game m_game => Game.Instance;
    protected GameLoader m_loader => GameLoader.Instance;
    public virtual void AddRetries(int amount) => m_game.retries += amount;
    public virtual void LoadScene(string scene) => m_loader.Load(scene);
}

