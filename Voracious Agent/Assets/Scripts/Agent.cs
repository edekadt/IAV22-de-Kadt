using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    protected class Action
    {
        public Action(ref Agent agent_, in float priority_ = float.MaxValue)
        {
            agent = agent_;
            priority = priority_;
        }

        // Amount that priority increases every second
        float increasePrioOverTime = 0f;

        // Warns the agent if another action CANNOT be started (the current action needs to be completed first)
        public bool lockAction = false;

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

        protected Agent agent;

        // To increase priority, pass negative values (lowest priority value -> will go next)
        protected virtual void AddPriority(float increase) { priority += increase; }
        protected virtual void SetPriority(float priority_) { priority = priority_; }
        
        private float priority;

        // This method is private to ensure that the passiveUpdate can be safely overloaded without breaking cooldowns
        public void PassiveUpdateAndPrio()
        {
            PassiveUpdate();
            AddPriority(-increasePrioOverTime * Time.deltaTime);
        }
    }

    protected Action AddAction(Action a)
    {
        actions.Add(a);
        return a;
    }

    // Iterable list of actions
    private IList<Action> actions;

    protected Action currentAction = null;

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
        //actions = new IList<Action>();
    }
    // Start is called before the first frame update
    void Start()
    {
        GetNextAction();
        if (currentAction == null)
            throw new System.Exception("Agent created with no actions.");
    }

    // Update is called once per frame
    void Update()
    {
        currentAction.ActiveUpdate();

        foreach (Action a in actions)
            a.PassiveUpdateAndPrio();

        if (!currentAction.lockAction)
        {
            Action previous = currentAction;
            GetNextAction();
            // Only call OnActionStart if the action changes
            if (currentAction != previous)
                currentAction.OnActionStart();
        }
    }
}
