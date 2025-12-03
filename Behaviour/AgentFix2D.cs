using UnityEngine;
using UnityEngine.AI;

public class AgentFix2D : MonoBehaviour
{
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // ensure it doesnt try to rotate in 3d
        agent.updateRotation = false; 
        agent.updateUpAxis = false;  
    }

    void Update()
    {   
        // get agent to face direction it is moving
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(agent.velocity.y, agent.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }
    }
}