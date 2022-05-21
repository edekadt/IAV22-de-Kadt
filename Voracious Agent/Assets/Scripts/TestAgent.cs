using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AggressiveAgent
{
    public class TestAgent : Agent
    {
        protected class Chase : Action
        {
            public Chase(Agent agent_): base (agent_) { }

            float maxVel = 20;

            override public void ActiveUpdate()
            {
                if (rigidbody.velocity.magnitude < maxVel)
                {
                    Vector3 targetPos = (target.position);
                    Vector3 pos = (transform.position);

                    Vector3 force = (targetPos - pos).normalized * (10f) * rigidbody.mass;
                    rigidbody.AddForce(force, ForceMode.Acceleration);
                }
            }
        }

        public GameObject projectile;
        protected class Attack : Action
        {
            public GameObject projectile;
            public Attack(Agent agent_) : base(agent_, -10) { }

            public override void OnActionStart()
            {
                GameObject p = Instantiate(projectile);
                Rigidbody rb = p.GetComponent<Rigidbody>();
                rb.velocity = new Vector3(0f, 10f, 0f);
            }

            public override bool Conditions()
            {
                return((target.position - transform.position).magnitude < 5);
            }
        }

        override protected void AgentAwake()
        {
            Chase chase = (Chase)(AddDefaultAction(new Chase(this)));
            chase.target = targets[0].GetComponent<Transform>();
            Attack attack = (Attack)(AddAction(new Attack(this)));
            attack.target = targets[0].GetComponent<Transform>();
            attack.projectile = projectile;
        }

        protected override void AgentStart()
        {
            
        }
    }
}