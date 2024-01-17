namespace Players.PlayerStates
{
    [System.Serializable]
    public abstract class StateMachineBase
    {
        public string stateName;
        public StateMachineController stateMachineController;

        public StateMachineBase(string name, StateMachineController stateMachineController)
        {
            stateName = name;
            this.stateMachineController = stateMachineController;
        }
        public abstract void StateEnter();
        public abstract void StateExit();
        public abstract void StateUpdate();
        public abstract void StatePhysicsUpdate();
        public abstract void StateLateUpdate();
    }
}
