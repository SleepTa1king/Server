using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
public class LevelRespawner:Singleton<LevelRespawner>
{

    public UnityEvent OnRespawn;
    public UnityEvent OnGameOver;
    protected List<PlayerCamera> m_cameras;
    protected Level m_level => Level.Instance;
    protected LevelPauser m_pauser => LevelPauser.Instance;
    protected Game m_game => Game.Instance;
    protected Fader m_fader => Fader.Instance;
    protected LevelScore m_score => LevelScore.Instance;

    public float respawnFadeOutDelay = 1f;
    public float respawnFadeInDelay = 1f;

    public virtual void Respawn(bool consumeRetries)
    {
        StopAllCoroutines();
        StartCoroutine(Routine(consumeRetries));
    }

    protected virtual IEnumerator Routine(bool consumeRetries)
    {
        m_pauser.Pause(false);
        m_pauser.canPause = false;
        m_level.player.inputs.enabled = false;
        if(consumeRetries && m_game.retries ==0)
        {
            //StartCoroutine(GameOverRoutine());
            yield break;
        }
        yield return new WaitForSeconds(respawnFadeOutDelay);
        m_fader.FadeOut(() => StartCoroutine(RespawnRoutine(consumeRetries)));  
    }

    //protected virtual IEnumerator GameOverRoutine()
    //{
    //    m_score.stopTime = true;
    //    yield return new WaitForSeconds(gameOverFadeOutDelay);
    //    GameLoader.Instance.Reload();
    //    OnGameOver?.invoke();
    //}

    protected virtual IEnumerator RespawnRoutine(bool consumeRetries)
    {
        if(consumeRetries)
        {
            m_game.retries--;
        }
        m_level.player.Respawn();
        m_score.coins = 0;
        ResetCamera();
        OnRespawn?.Invoke();

        yield return new WaitForSeconds(respawnFadeInDelay);
        m_fader.FadeIn(() =>
        {
            m_pauser.canPause = true;
            m_level.player.inputs.enabled = true;
        });
    }

    protected virtual void ResetCamera()
    {
        foreach(var camera in m_cameras)
        {
            camera.Reset();
        }
    }

    protected virtual void Start()
    {
        m_cameras = new List<PlayerCamera>(FindObjectsByType<PlayerCamera>(FindObjectsSortMode.None));
        m_level.player.playerEvents.OnDie.AddListener(() => Respawn(true));
    }
}

