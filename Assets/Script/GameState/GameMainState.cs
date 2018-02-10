﻿using System;
using Utility;

namespace GameFramework
{
	internal class GameMainState : State<GameStateMachine>
	{
        public GameMainState()
        {

        }

        public GameMainState( GameStateMachine _owner, GameStateMachine _fsm, String _name ) : base( _owner, _fsm, _name )
        {

        }

		public override void OnEnter()
		{
            
		}
	}
}