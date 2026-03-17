using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Player))]
public class PlayerFootSteps : MonoBehaviour
{
    [System.Serializable]
    public class Surface
    {
        public string tag;
        public AudioClip[] footsteps;
        public AudioClip[] landings;
    }
    public Surface[] surfaces;
    public AudioClip[] defaultFootsteps;
    public AudioClip[] defaultLandings;

    protected Player m_player;
    protected AudioSource m_audio;
    protected Vector3 m_lastLateralPositon;
    public float stepOffset = 1.25f;
    public float footstepVolume = 0.5f;

    protected Dictionary<string, AudioClip[]> m_footsteps = new Dictionary<string, AudioClip[]>();

    protected Dictionary<string, AudioClip[]> m_landings = new Dictionary<string, AudioClip[]>();
    protected virtual void Start()
    {
        m_player = GetComponent<Player>();
        m_player.entityEvents.OnGroundEnter?.AddListener(Landing);

        if(!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();

        }
        foreach(var surface in surfaces)
        {
            m_footsteps.Add(surface.tag, surface.footsteps);
            m_landings.Add(surface.tag, surface.landings);
        }
    }

    protected virtual void Update()
    {
        if(m_player.isGrounded && m_player.states.IsCurrentOfState(typeof(WalkPlayerState)))
        {
            var position = transform.position;
            var lateralPosition = new Vector3(position.x, 0, position.z);
            var distance = (m_lastLateralPositon - lateralPosition).magnitude;

            if(distance >= stepOffset)
            {
                if (m_footsteps.ContainsKey(m_player.groundHit.collider.tag))
                {
                    PlayRandomClip(m_footsteps[m_player.groundHit.collider.tag]);
                }
                else
                {
                    PlayRandomClip(defaultFootsteps);
                }
                m_lastLateralPositon = lateralPosition;
            }
        }
    }

    protected virtual void PlayRandomClip(AudioClip[] clips)
    {
        if(clips.Length > 0)
        {
            var index = Random.Range(0, clips.Length);
            m_audio.PlayOneShot(clips[index],footstepVolume);
        }
    }

    protected virtual void Landing()
    {
        if(!m_player.onWater)
        {
            if(m_landings.ContainsKey(m_player.groundHit.collider.tag))
            {
                PlayRandomClip(m_landings[m_player.groundHit.collider.tag]);
            }
            else
            {
                PlayRandomClip(defaultLandings);
            }

        }
    }
}

