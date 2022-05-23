using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace AggressiveAgent
{
    public class SoulMaster : Agent
    {
        protected class TeleportationAction : Action
        {
            protected TeleportationAction(Agent agent_, float prio) : base(agent_, prio) { }
            protected void Teleport(Vector3 destination)
            {
                rigidbody.Sleep();
                rigidbody.position = destination;
                rigidbody.WakeUp();
            }
        }
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
        protected class Blink : TeleportationAction
        {
            public Blink(Agent agent_): base(agent_, -5) 
            {
                target = GetSharedObject("Knight").transform;
            }
            
            public override void OnActionStart()
            {
                float x = Random.Range(-20, 20), z = Random.Range(-20, 20);
                Teleport(new Vector3(x, rigidbody.position.y, z));
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

        protected class Slam : TeleportationAction
        {
            public Slam(Agent agent_) : base(agent_, -3)
            {
                target = GetSharedObject("Knight").transform;
                startingHeight = transform.position.y;
            }
            float distanceAbove = 10;
            float startingHeight;

            public override void OnActionStart()
            {
                lockAction = true;
                Teleport(target.position + new Vector3(0, distanceAbove, 0));
                rigidbody.AddForce(new Vector3(0, 3, 0), ForceMode.Impulse);
                agent.StartCoroutine(GroundSlam());
            }

            public override void ActiveUpdate()
            {
                Vector3 aimAdjustment = (target.position - transform.position);
                aimAdjustment.y = 0;
                aimAdjustment.Normalize();
                rigidbody.AddForce(aimAdjustment * 2, ForceMode.Acceleration);
            }

            public override void OnCollision(Collision collision)
            {
                Teleport(new Vector3(0f, -25f, 0f));
                agent.StartCoroutine(ReturnToMap());
            }

            private IEnumerator GroundSlam()
            {
                yield return new WaitForSeconds(0.5f);
                rigidbody.AddForce(new Vector3(0, -60, 0), ForceMode.Impulse);
            }

            private IEnumerator ReturnToMap()
            {
                yield return new WaitForSeconds(1.5f);
                float x = Random.Range(-20, 20), z = Random.Range(-20, 20);
                Teleport(new Vector3(x, startingHeight, z));
                cooldown = 15f;
                lockAction = false;
            }
        }

        override protected void AgentStart()
        {
            //printCurrentAction = true;
            Hover hover = (Hover)(AddDefaultAction(new Hover(this)));
            Blink teleport = (Blink)(AddAction(new Blink(this)));
            Shoot shoot = (Shoot)(AddAction(new Shoot(this)));
            Slam slam = (Slam)(AddAction(new Slam(this)));
            slam.AddSetupAction(shoot, 4);
        }
    }
}