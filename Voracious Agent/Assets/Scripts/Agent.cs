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
                transform = agent_.GetComponent<Transform>();
                rigidbody = agent_.GetComponent<Rigidbody>();
            }

            /// <summary>
            /// Conditions that must be fulfilled for the action to be performed
            /// For example, a melee attack might require a minimum proximity to a target
            /// </summary>
            public virtual bool Conditions() { return true; }

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
            /// If the action has not finished, it has higher priority than any other
            /// If the conditions are not fulfilled, it has lower priority than any other
            /// </summary>
            public float getPriority()
            {
                return lockAction ? float.MinValue : (Conditions() || isDefault) && cooldown <= 0f ? priority : float.MaxValue;
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
            /// This method is private to ensure that the passiveUpdate can be safely overloaded without breaking cooldowns
            /// </summary>
            public void PassiveUpdateAndCooldown()
            {
                PassiveUpdate();
                if (cooldown > 0f) cooldown -= Time.deltaTime;
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

            // References for child classes to use
            protected Transform transform = null;
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
                    currentAction.OnActionStart();
            }
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
    }
}