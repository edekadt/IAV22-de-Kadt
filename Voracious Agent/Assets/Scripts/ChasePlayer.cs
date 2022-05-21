using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AggressiveAgent
{
    public class ChasePlayer : Agent
    {
        protected class Chase : Action
        {
            public Chase(Agent agent_): base (agent_) { }

            float maxVel = 50;

            override public void ActiveUpdate()
            {
                Debug.Log("ActiveUpdate");
                if (rigidbody.velocity.magnitude < maxVel)
                {
                    Vector3 targetPos = (target.position);
                    Vector3 pos = (transform.position);

                    Vector3 force = (targetPos - pos).normalized * (10f) * rigidbody.mass;
                    rigidbody.AddForce(force, ForceMode.Acceleration);

                    //look at target
                    //Utilities::Vector3<float> dir = targetPos - pos;
                    //dir.normalize();
                    //float angle = std::atan2(dir.x, dir.z);
                    //rb->setRotation(Utilities::Vector3<int>(0, 1, 0), angle);
                }
            }
        }
        override protected void AgentAwake()
        {
            Chase chase = (Chase)(AddDefaultAction(new Chase(this)));
            chase.target = targets[0].GetComponent<Transform>();
        }

        protected override void AgentStart()
        {
            
        }
    }
}