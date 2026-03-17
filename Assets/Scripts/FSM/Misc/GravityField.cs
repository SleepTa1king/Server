using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GravityField : MonoBehaviour
{
    public float force = 75f;
    protected Collider m_collier;
    private void Start()
    {
        m_collier = GetComponent<Collider>();
        m_collier.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag(GameTags.Player))
        {
            if(other.TryGetComponent<Player>(out var player))
            {
                if(player.isGrounded)
                {
                    player.verticalVelocity = Vector3.zero;
                }
                player.verticalVelocity += transform.up * force * Time.deltaTime;
            }
        }
    }
}

