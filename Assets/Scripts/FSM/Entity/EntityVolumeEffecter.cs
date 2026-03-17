using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EntityVolumeEffecter:MonoBehaviour
{
    protected Collider m_collier;

    public float velocityConversion = 1f;
    public float accelerationMultiplier = 1f;
    public float topSpeedMultiplier = 1f;
    public float decelerationMultiplier = 1f;
    public float turningDragMultiplier = 1f;
    public float gravityMultiplier = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out EntityBase entity))
        {
            entity.velocity *= velocityConversion;
            entity.accelerationMultiplier = accelerationMultiplier;
            entity.topSpeedMultiplier = topSpeedMultiplier;
            entity.decelerationMultiplier = decelerationMultiplier;
            entity.turningDragMultiplier = turningDragMultiplier;
            entity.gravityMultiplier = gravityMultiplier;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out EntityBase entity))
        {
            entity.accelerationMultiplier = 1f;
            entity.topSpeedMultiplier = 1f;
            entity.decelerationMultiplier = 1f;
            entity.turningDragMultiplier = 1f;
            entity.gravityMultiplier = 1f;
        }
    }
    private void Start()
    {
        m_collier = GetComponent<Collider>();
        m_collier.isTrigger = true;
    }
}

