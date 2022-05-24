using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
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
                lockAction = true;
                GameObject seekerInstance = Instantiate(seeker, forehead.position, Quaternion.identity);
                seekerInstance.GetComponent<Seek>().target = target;
                Vector3 impulse = 10 * (new Vector3(target.position.x, transform.position.y, target.position.z) - transform.position).normalized;
                seekerInstance.GetComponent<Rigidbody>().AddForce(impulse, ForceMode.Impulse);
                cooldown = 2f;
                UnlockIn(0.4f);
            }
        }

        protected class Slam : TeleportationAction
        {
            public Slam(Agent agent_) : base(agent_, -3)
            {
                target = GetSharedObject("Knight").transform;
                startingHeight = transform.position.y;
                faceTarget = agent.gameObject.GetComponent<FaceTarget>();
                crashParticles = GetSharedObject("CrashParticles").GetComponent<ParticleSystem>();
            }
            FaceTarget faceTarget;
            ParticleSystem crashParticles;
            float distanceAbove = 10;
            float startingHeight;

            public override void OnActionStart()
            {
                lockAction = true;
                faceTarget.enabled = false;
                Teleport(target.position + new Vector3(0, distanceAbove, 0));
                rigidbody.AddForce(new Vector3(0, 12, 0), ForceMode.Impulse);
                StartCoroutine(GroundSlam());
            }

            public override void ActiveUpdate()
            {
                Vector3 aimAdjustment = (target.position - transform.position);
                aimAdjustment.y = 0;
                aimAdjustment.Normalize();
                rigidbody.AddForce(aimAdjustment * 2, ForceMode.Acceleration);
            }

            public override void OnTrigger(Collider other)
            {
                Teleport(new Vector3(0f, -25f, 0f));
                StartCoroutine(ReturnToMap());
                faceTarget.enabled = true;
                crashParticles.gameObject.transform.position = transform.position;
                crashParticles.Play();
            }

            private IEnumerator GroundSlam()
            {
                float rotation = 0f;
                float time = 0f;
                float turnDuration = 0.8f;
                while (time < turnDuration)
                {
                    time += Time.deltaTime;
                    transform.Rotate(new Vector3(1f, 0f, 0f), Time.deltaTime * 110f / turnDuration);
                    yield return null;
                }
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

        protected class CircleChase : TeleportationAction
        {
            public CircleChase(Agent agent_) : base(agent_, -6)
            {
                target = GetSharedObject("Knight").transform;
                circle = GetSharedObject("Circle");
                circle.transform.localScale = new Vector3(0f, 0f, 0f);
                faceTarget = agent.gameObject.GetComponent<FaceTarget>();
            }

            GameObject circle;
            float deployTime = 1.8f;
            FaceTarget faceTarget;

            public override void OnActionStart()
            {
                lockAction = true;
                float x = Random.Range(-20, 20), z = Random.Range(-20, 20);
                Teleport(new Vector3(x, rigidbody.position.y, z));
                StartCoroutine(DeployOrbs());
                cooldown = 8f;
            }

            public override bool Conditions()
            {
                return Mathf.Abs(target.position.x) < 10f && Mathf.Abs(target.position.z) < 10f;
            }

            private IEnumerator DeployOrbs()
            {
                float scale = 0f;
                while (scale < 1f)
                {
                    scale += Time.deltaTime / deployTime;
                    circle.transform.localScale = new Vector3(scale, scale, scale);
                    yield return null;
                }
                StartCoroutine(CrossArena());
            }

            private IEnumerator CrossArena()
            {
                faceTarget.enabled = false;
                Vector3 direction = (target.position - transform.position);
                direction.y = 0f;
                direction.Normalize();
                while (Mathf.Abs(transform.position.x) < 21 && Mathf.Abs(transform.position.z) < 21)
                {
                    rigidbody.velocity = direction * 16;
                    yield return null;
                }
                faceTarget.enabled = true;
                circle.transform.localScale = new Vector3(0f, 0f, 0f);
                rigidbody.velocity = new Vector3(0f, 0f, 0f);
                UnlockIn(0.4f);
            }
        }

        override protected void AgentStart()
        {
            //printCurrentAction = true;
            AddDefaultAction(new Hover(this));
            AddAction(new Blink(this));
            Shoot shoot = (Shoot)(AddAction(new Shoot(this)));
            Slam slam = (Slam)(AddAction(new Slam(this)));
            slam.AddSetupAction(shoot, 4);
            AddAction(new CircleChase(this)).AddSetupAction(slam);
        }
    }
}