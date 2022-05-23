using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AggressiveAgent
{
    public class SoulMaster : Agent
    {
        protected class Hover : Action
        {
            public Hover(Agent agent_) : base(agent_) 
            {
                startingHeight = transform.position.y + 1;
            }

            float vertBobbing = 4f;
            float startingHeight;

            override public void ActiveUpdate()
            {
                if (transform.position.y > startingHeight)
                    rigidbody.AddForce(new Vector3(0, -vertBobbing, 0), ForceMode.Acceleration);
                else
                    rigidbody.AddForce(new Vector3(0, vertBobbing, 0), ForceMode.Acceleration);
            }
        }
        protected class Teleport : Action
        {
            public Teleport(Agent agent_): base(agent_, -5) 
            {
                target = GetSharedObject("Knight").transform;
            }
            
            public override void OnActionStart()
            {
                float x = Random.Range(-20, 20), z = Random.Range(-20, 20);
                rigidbody.Sleep();
                rigidbody.position = new Vector3(x, rigidbody.position.y, z);
                rigidbody.WakeUp();
                cooldown = 8f;
            }

            public override bool Conditions()
            {
                return (transform.position - target.position).magnitude < 10f;
            }
        }

        protected class Shoot : Action
        {
            public Shoot(Agent agent_) : base(agent_, -1)
            {
                target = GetSharedObject("Knight").transform;
                forehead = GetSharedObject("Forehead").transform;
                seeker = GetSharedObject("Seeker");
            }

            GameObject seeker;
            Transform forehead;
            public override void OnActionStart()
            {
                GameObject seekerInstance = Instantiate(seeker, forehead.position, Quaternion.identity);
                seekerInstance.GetComponent<Seek>().target = target;
                Vector3 impulse = 10 * (new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position).normalized;
                seekerInstance.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.Impulse);
                cooldown = 2f;
            }
        }

        override protected void AgentStart()
        {
            //printCurrentAction = true;
            Hover hover = (Hover)(AddDefaultAction(new Hover(this)));
            Teleport teleport = (Teleport)(AddAction(new Teleport(this)));
            Shoot shoot = (Shoot)(AddAction(new Shoot(this)));
        }
    }
}