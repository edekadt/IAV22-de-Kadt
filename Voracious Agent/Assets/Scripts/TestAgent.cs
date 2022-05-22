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

        protected class Attack : Action
        {
            public GameObject projectile;
            public Attack(Agent agent_) : base(agent_, -10) 
            {
                projectile = GetSharedObject("Projectile");
            }

            public override void OnActionStart()
            {
                GameObject p = Instantiate(projectile, transform.position, Quaternion.identity);
                Rigidbody rb = p.GetComponent<Rigidbody>();
                rb.velocity = new Vector3(0f, 2f, 0f);
                cooldown = 3f;
            }

            public override bool Conditions()
            {
                return((target.position - transform.position).magnitude < 2);
            }
        }

        protected class Duplicate: Action
        {
            public Duplicate(Agent agent_): base(agent_, -50) 
            {
                cooldown = 4f;
                duplicate = GetSharedObject("Duplicate");
                projectile = GetSharedObject("Projectile");
            }

            public GameObject duplicate;
            public GameObject projectile;
            float duplicationShunt = 15f;
            int numDuplicates = 1;

            public override void OnActionStart()
            {
                for (int i = 0; i < numDuplicates; ++i)
                {
                    GameObject d = Instantiate(duplicate, transform.position, Quaternion.identity);
                    Rigidbody rb = d.GetComponent<Rigidbody>();
                    Vector3 shunt = new Vector3(duplicationShunt, 0f, 0f);
                    shunt = Quaternion.AngleAxis(360/numDuplicates * i, Vector3.up) * shunt;
                    rb.velocity = shunt;

                    TestAgent ta = d.GetComponent<TestAgent>();
                    ta.sharedObjects = new SerializableDictionary(in agent.sharedObjects);
                }
                cooldown = 20f;
            }

            public override bool Conditions()
            {
                return ((target.position - transform.position).magnitude < 6);
            }
        }

        override protected void AgentStart()
        {
            Chase chase = (Chase)(AddDefaultAction(new Chase(this)));
            chase.target = sharedObjects["Player"].GetComponent<Transform>();

            Attack attack = (Attack)(AddAction(new Attack(this)));
            attack.target = sharedObjects["Player"].GetComponent<Transform>();

            Duplicate dupe = (Duplicate)(AddAction(new Duplicate(this)));
            dupe.target = sharedObjects["Player"].GetComponent<Transform>();
        }
    }
}