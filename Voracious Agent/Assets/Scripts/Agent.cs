using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

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

            // Conditions that must be fulfilled for the action to be performed
            // For example, a melee attack might require a minimum proximity to a target
            public virtual bool Conditions() { return true; }

            // Called once each time the action is performed, at the moment it begins
            public virtual void OnActionStart() { }

            // Called every frame, regardless of whether action is being performed (necessary for things like controlling priority)
            public virtual void PassiveUpdate() { }

            // Called every frame while the action is being performed
            public virtual void ActiveUpdate() { }

            // If the action has not finished, it has higher priority than any other
            // If the conditions are not fulfilled, it has lower priority than any other
            public float getPriority()
            {
                return lockAction ? float.MinValue : Conditions() ? priority : float.MaxValue;
            }

            // To increase priority, pass negative values (lowest priority value -> will go next)
            protected virtual void AddPriority(float increase) { priority += increase; }
            protected virtual void SetPriority(float priority_) { priority = priority_; }
            public virtual void SetDefault() { priority = 0f; increasePrioOverTime = 0f; }

            // This method is private to ensure that the passiveUpdate can be safely overloaded without breaking cooldowns
            public void PassiveUpdateAndPrio()
            {
                PassiveUpdate();
                AddPriority(-increasePrioOverTime * Time.deltaTime);
            }

            protected Agent agent;

            private float priority;

            // Not necessary for all actions, but many involve a target
            public Transform target = null;

            // References for child classes to use
            protected Transform transform = null;
            protected Rigidbody rigidbody = null;

            // Amount that priority increases every second
            public float increasePrioOverTime = 0f;

            // Warns the agent if another action CANNOT be started (the current action needs to be completed first)
            public bool lockAction = false;
        }

        protected Action AddAction(Action a)
        {
            Debug.Log("Adding action " + a);
            actions.Add(a);
            return a;
        }
        protected Action AddDefaultAction(Action a)
        {
            Debug.Log("Adding default action " + a);
            a.SetDefault();
            return AddAction(a);
        }

        // Chooses the next action to be performed
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
            AgentAwake();
        }

        // Start is called before the first frame update
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
                if (a.getPriority() == 0f && a.increasePrioOverTime == 0f)
                {
                    defaultAction = a;
                    break;
                }
            }
        }

        // Overloadable methods for child classes to use
        abstract protected void AgentStart();
        abstract protected void AgentAwake();

        // Update is called once per frame
        void Update()
        {
            Debug.Log("Current action: " + currentAction);
            currentAction.ActiveUpdate();

            foreach (Action a in actions)
                a.PassiveUpdateAndPrio();

            if (!currentAction.lockAction && actions.Count > 1)
            {
                Action previous = currentAction;
                GetNextAction();
                // Only call OnActionStart if the action changes
                if (currentAction != previous)
                    currentAction.OnActionStart();
            }
        }

        // Other objects that the agent takes into account
        public GameObject[] targets;

        // Iterable list of actions
        private IList<Action> actions = new List<Action>();

        protected Action currentAction = null;
        protected Action defaultAction = null;

    }
}