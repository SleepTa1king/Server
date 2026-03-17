using UnityEngine;

[RequireComponent(typeof(WaypointManager))]
[RequireComponent(typeof(Collider))]
public class MovingPlatform:MonoBehaviour
{
    [Header("移动设置")]
    public float speed = 3f;
    public float offset = 0.1f;
    protected WaypointManager waypoints;
    private void Awake()
    {
        tag = GameTags.Platform;
        waypoints = GetComponent<WaypointManager>();
    }

    private void Update()
    {
        var position = transform.position;
        var target = waypoints.current.position;

        position = Vector3.MoveTowards(position, target, speed * Time.deltaTime);
        transform.position = position;

        if(Vector3.Distance(position,target)<=offset)
        {
            waypoints.Next();
        }
    }
}

