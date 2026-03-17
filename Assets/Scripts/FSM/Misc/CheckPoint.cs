using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class CheckPoint:MonoBehaviour
{
    public Transform respawn;
    public UnityEvent OnActive;
    public AudioClip clip;
    protected AudioSource m_audio;
    protected Collider m_collider;
    public bool activated { get; protected set; }
    private void Awake()
    {
        if(!TryGetComponent(out m_audio))
        {
            m_audio = GetComponent<AudioSource>();
        }
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!activated && other.TryGetComponent<Player>(out var player))
        {
            Activate(player);
        }
    }

    public virtual void Activate(Player player)
    {
        if(!activated)
        {
            activated = true;
            m_audio.PlayOneShot(clip);
            player.SetRespawn(respawn.position, respawn.rotation);
            OnActive?.Invoke();
        }
    }
}

