using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace AggressiveAgent
{
    abstract public class Agent : MonoBehaviour
    {
        protected class Action
        {
            public Action(Agent agent_, in float priority_ = float.MaxValue)
            {
                agent = agent_;
                priority = priority_;
                rigidbody = agent_.GetComponent<Rigidbody>();
            }

            /// <summary>
            /// Conditions that must be fulfilled for the action to be performed
            /// For example, a melee attack might require a minimum proximity to a target
            /// </summary>
            public virtual bool Conditions() { return true; }

            /// <summary>
            /// Accounts for cooldowns, set-up actions and scriptable conditions
            /// </summary>
            private bool _AllConditions()
            {
                return (Conditions() || isDefault) && cooldown <= 0f && allSetUpsComplete;
            }

            /// <summary>
            /// Called once each time the action is performed, at the moment it begins
            /// </summary>
            public virtual void OnActionStart() { }

            /// <summary>
            /// Called every frame, regardless of whether action is being performed (necessary for things like controlling priority)
            /// </summary>
            public virtual void PassiveUpdate() { }

            /// <summary>
            /// Called every frame while the action is being performed
            /// </summary>
            public virtual void ActiveUpdate() { }

            /// <summary>
            /// Called when the agent detects a collision while this action is being performed
            /// </summary>
            /// <param name="collision"></param>
            public virtual void OnCollision(Collision collision) { }
            /// <summary>
            /// Called when the agent detects a trigger entry while this action is being performed
            /// </summary>
            /// <param name="collision"></param>
            public virtual void OnTrigger(Collider other) { }

            /// <summary>
            /// If the action has not finished, it has higher priority than any other
            /// If the conditions are not fulfilled, it has lower priority than any other
            /// </summary>
            public float getPriority()
            {
                return lockAction ? float.MinValue : _AllConditions() ? priority : float.MaxValue;
            }

            /// <summary>
            /// To increase priority, pass negative values (lowest priority value -> will go next)
            /// </summary>
            protected virtual void AddPriority(float increase) { priority += increase; }

            /// <summary>
            /// Lowest priority value will go next
            /// </summary>
            protected virtual void SetPriority(float priority_) { priority = priority_; }

            /// <summary>
            /// Makes the action valid as a default action, but DOES NOT make it the default action of an agent
            /// </summary>
            public virtual void SetDefaultValues() { isDefault = true; priority = 0f; hasCooldown = false; }

            /// <summary>
            /// Adds an action that must be performed at least once before each time this action can be performed.
            /// To require multiple uses of the action beforehand, use the second parameter.
            /// </summary>
            public void AddSetupAction(Action action, uint n = 1)
            {
                allSetUpsComplete = false;

                SetUpAction setUp;
                setUp.action = action;
                setUp.necessaryCount = n;
                setUp.count = 0;
                setUpActions.Add(setUp);

                action.AddFollowUpAction(this);
            }

            /// <summary>
            /// Adds an action that must notify this one each time it is performed
            /// </summary>
            /// <param name="action"></param>
            protected void AddFollowUpAction(Action action)
            {
                followUpActions.Add(action);
            }

            /// <summary>
            /// This method exists to ensure that PassiveUpdate() can be safely overloaded without breaking cooldowns
            /// </summary>
            public void PassiveUpdateAndCooldown()
            {
                PassiveUpdate();
                if (cooldown > 0f) cooldown -= Time.deltaTime;
            }

            /// <summary>
            /// Notifies all follow-up action of this action starting
            /// </summary>
            public void NotifyChains()
            {
                foreach(Action a in followUpActions)
                {
                    a.OnSetupAction(this);
                }
            }

            /// <summary>
            /// Upon starting the action, reset the count of all set up actions
            /// </summary>
            public void ResetSetUp()
            {
                allSetUpsComplete = setUpActions.Count == 0;
                for (int i = 0; i < setUpActions.Count; ++i)
                    setUpActions[i].Reset();
            }

            /// <summary>
            /// When one of the action's set-ups begins, check to see if action is available now
            /// </summary>
            /// <param name="a"></param>
            protected void OnSetupAction(Action a)
            {
                int i = 0;
                while (i < setUpActions.Count && setUpActions[i].action != a) { ++i; }
                if (i == setUpActions.Count) throw new System.Exception("Action notified by unrecognized set-up action.");

                if ((++setUpActions[i]).count >= setUpActions[i].necessaryCount)
                {
                    allSetUpsComplete = true;
                    foreach (SetUpAction setUp in setUpActions)
                        if (setUp.count < setUp.necessaryCount)
                        {
                            allSetUpsComplete = false;
                            break;
                        }
                }
            }

            /// <summary>
            /// Starts a coroutine
            /// </summary>
            /// <param name="coroutine"></param>
            protected void StartCoroutine(IEnumerator coroutine)
            {
                agent.StartCoroutine(coroutine);
            }

            /// <summary>
            /// Unlocks the action after a number of seconds
            /// </summary>
            /// <returns></returns>
            protected void UnlockIn(float time)
            {
                StartCoroutine(_unlockAction(time));
            }

            private IEnumerator _unlockAction(float time)
            {
                yield return new WaitForSeconds(time);
                lockAction = false;
            }

            /// <summary>
            /// Find an object from the agent's pool of shared objects
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            protected GameObject GetSharedObject(string key)
            {
                return agent.sharedObjects[key];
            }

            protected Agent agent;

            private float priority;

            // Not necessary for all actions, but many involve a target
            public Transform target = null;

            // Reference for child classes to use
            protected Rigidbody rigidbody = null;

            /// <summary>
            /// True if the action can be given a cooldown
            /// </summary>
            public bool hasCooldown { get; private set; } = false;

            private float _cooldown = 0f;
            /// <summary>
            /// Time remaining before the action can be started again
            /// </summary>
            public float cooldown {
                get 
                {
                    return _cooldown; 
                } 
                set 
                {
                    if (isDefault)
                        throw new System.Exception("Cannot set cooldown for default action.");
                    _cooldown = value; 
                    if (value != 0) hasCooldown = true; 
                } 
            }

            /// <summary>
            /// Indicates whether the action is default
            /// Default actions cannot be given cooldowns, and are always considered valid to perform
            /// </summary>
            bool isDefault = false;

            /// <summary>
            /// Warns the agent if another action CANNOT be started (the current action needs to be completed first)
            /// </summary>
            public bool lockAction = false;


            /// <summary>
            /// List of actions that can´t be performed before this one
            /// </summary>
            List<Action> followUpActions = new List<Action>();

            struct SetUpAction
            {
                public Action action;
                public uint necessaryCount;
                public uint count;
                public void Reset()
                {
                    count = 0;
                }
                private SetUpAction(Action a, uint _necessary, uint _count) { action = a; necessaryCount = _necessary; count = _count; }
                public static SetUpAction operator ++(SetUpAction a) => new SetUpAction(a.action, a.necessaryCount, a.count + 1);
            }
            /// <summary>
            /// List of actions that must be performed at least once before each time this one is
            /// </summary>
            List<SetUpAction> setUpActions = new List<SetUpAction>();

            /// <summary>
            /// Indicated whether all set-up action have been performed enough times
            /// </summary>
            private bool allSetUpsComplete = true;

            public Transform transform { get { return agent.gameObject.transform; } }
        }

        protected Action AddAction(Action a)
        {
            actions.Add(a);
            return a;
        }
        protected Action AddDefaultAction(Action a)
        {
            if (defaultAction != null)
                throw new System.Exception("Agent cannot have 2 default actions.");
            a.SetDefaultValues();
            return AddAction(a);
        }

        /// <summary>
        /// Chooses the next action to be performed
        /// </summary>
        private Action GetNextAction()
        {
            float highestPrio = float.MaxValue;
            foreach (Action a in actions)
            {
                if (a.getPriority() < highestPrio)
                {
                    currentAction = a;
                    highestPrio = a.getPriority();
                }
            }
            return currentAction;
        }

        private void Awake()
        {
            sharedObjects.Initialize();
            AgentAwake();
        }

        void Start()
        {
            AgentStart();
            GetNextAction();
            if (currentAction == null)
                throw new System.Exception("Agent created with no actions.");
            
            if (defaultAction == null) FindDefaultAction();
            if (defaultAction == null)
                throw new System.Exception("Agent created with no default action.");
            currentAction.OnActionStart();
        }

        private void OnCollisionEnter(Collision collision)
        {
            AgentOnCollision(collision);
            currentAction.OnCollision(collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            AgentOnTrigger(other);
            currentAction.OnTrigger(other);
        }


        // If a default action has not been set, we find one that works as default
        private void FindDefaultAction()
        {
            foreach (Action a in actions)
            {
                if (a.getPriority() == 0f && !a.hasCooldown)
                {
                    defaultAction = a;
                    break;
                }
            }
        }

        // Overloadable methods for child classes to use
        virtual protected void AgentStart() { }
        virtual protected void AgentAwake() { }
        virtual protected void AgentOnCollision(Collision collision) { }
        virtual protected void AgentOnTrigger(Collider other) { }

        void Update()
        {
            currentAction.ActiveUpdate();

            foreach (Action a in actions)
                a.PassiveUpdateAndCooldown();

            if (!currentAction.lockAction && actions.Count > 1)
            {
                Action previous = currentAction;
                GetNextAction();
                // Only call OnActionStart if the action changes
                if (currentAction != previous)
                {
                    currentAction.OnActionStart();
                    currentAction.NotifyChains();
                    currentAction.ResetSetUp();
                }
            }
            if (printCurrentAction)
                Debug.Log("Current action: " + currentAction);
        }

        /// <summary>
        /// A pool of gameobjects that any action can access
        /// </summary>
        public SerializableDictionary sharedObjects;

        /// <summary>
        /// Iterable list of actions
        /// </summary>
        private IList<Action> actions = new List<Action>();

        protected Action currentAction = null;
        protected Action defaultAction = null;

        /// <summary>
        /// If true, will write the current action in Unity console every frame
        /// </summary>
        public bool printCurrentAction = false;
    }
}