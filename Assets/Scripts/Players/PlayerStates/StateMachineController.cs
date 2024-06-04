using Players.PlayerStates.States;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Players.PlayerStates
{
    public class StateMachineController : NetworkBehaviour
    {
        [Tooltip("All my states")]
        public StateMachineBase[] states;
        public StateMachineBase [] weaponStates { get; private set; }
        
        public StateMachineBase currentState { get; private set; }
        public string lastStateName { get; private set; }
        public StateMachineBase myWeaponState { get; private set; }

        [Header("Player States")]
        public MovementState movementState;
        public JumpState jumpState;
        public SprintState sprintState;
        public CrouchState crouchState;
        public SlidingState slidingState;
        public AimingState AimingState;
        public FallingState FallingState;
        public JetpackState jetpackState;
        public DeadState deadState;
        public IdleState idleState;
        public ViewerState viewerState;
        
        [Header("Weapon States")]
        public ChangingWeaponState changingWeaponState;
        public OnWeaponState onWeaponState;
        

        public NetworkAnimator networkAnimator;

        public bool changingWeapon; 
        public void Initializate()
        {
            movementState = new MovementState("Movement", this);
            jumpState = new JumpState("Jump", this);
            sprintState = new SprintState("Sprint", this);
            crouchState = new CrouchState("Crouch", this);
            slidingState = new SlidingState("Sliding", this);
            AimingState = new AimingState("Aiming", this);
            FallingState= new FallingState("Falling", this);
            jetpackState = new JetpackState("Jetpack", this);
            deadState = new DeadState("Dead", this);
            idleState = new IdleState("Idle", this);
            viewerState = new ViewerState("Viewer", this);
            
            
            changingWeaponState = new ChangingWeaponState("ChangingWeapon", this);
            onWeaponState = new OnWeaponState("OnWeapon", this);
            


            states =new StateMachineBase[11];
            states[0]=movementState;
            states[1]=jumpState;
            states[2]=sprintState;
            states[3]=crouchState;
            states[4]=slidingState;
            states[5]=AimingState;
            states[6]=FallingState;
            states[7]=jetpackState;
            states[8]=deadState;
            states[9]=idleState;
            states[10]=viewerState;
            
            weaponStates = new StateMachineBase[2];
            
            weaponStates[0] = onWeaponState;
            weaponStates[1] = changingWeaponState;

            // for (int i = 0; i < states.Length; i++)
            // {
            //     states[i].stateMachineController = this;
            // }


            
            if (states != null && states.Length > 0)
            {
                string initialStateName = states[0].stateName;
                SetState(initialStateName);
            }
            
            if (weaponStates != null && weaponStates.Length > 0)
            {
                string initialStateName = weaponStates[0].stateName;
                SetWeaponState(initialStateName);
            }
        }


        public void StateUpdate()
        {

            if (currentState != null )
            {
                currentState.StateUpdate();
                // Debug.Log(currentState.stateName);
                myWeaponState.StateUpdate();
               
            }
        }

        public void StatePhysicsUpdate()
        {
            if (currentState != null)
            {
                currentState.StatePhysicsUpdate();
                myWeaponState.StatePhysicsUpdate();
            }
        }

        public void StateLateUpdate()
        {
            if (currentState != null)
            {
                currentState.StateLateUpdate();
                myWeaponState.StateLateUpdate();

            }
        }
        public void SetState(string statename)
        {
            StateMachineBase nextState = GetStateWithName(statename, states);
            if (nextState == null) return;
            //Exit state execution
            if (currentState != null)
            {
                string lastStateTemp = currentState.stateName;
                lastStateName = lastStateTemp;
                currentState.StateExit();
            }
            //New state
            currentState = nextState;
            //Entry state execution
            currentState.StateEnter();
        }
        public void SetWeaponState(string statename)
        {
            StateMachineBase nextState = GetStateWithName(statename, weaponStates);
            if (nextState == null) return;
            //Exit state execution
            if (myWeaponState != null)
            {
                myWeaponState.StateExit();
            }
            //New state
            myWeaponState = nextState;
            //Entry state execution
            myWeaponState.StateEnter();
        }
        public void SetChangingWeaponState(WeaponItem weaponItem, string statename)
        {
            changingWeaponState.weaponToChange = weaponItem;
            SetWeaponState(statename);
        }

        private StateMachineBase GetStateWithName(string stateName, StateMachineBase[] stateMachine)
        {
            foreach (StateMachineBase state in stateMachine)
            {
                if (state.stateName == stateName)
                {
                    return state;
                }
            }
            return null;
        }
    }
}
