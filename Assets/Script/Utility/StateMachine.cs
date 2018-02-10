using System;
using System.Collections.Generic;

namespace Utility
{
	/// <summary>
	/// 简单状态机
	/// </summary>
	/// <typeparam name="_TyOwner"></typeparam>
	internal abstract class StateMachine<_TyOwner> where _TyOwner : class, new()
	{
		private Boolean m_running;

		/// <summary>
		/// 当前状态
		/// </summary>
		private State<_TyOwner> m_current_state;


		/// <summary>
		/// 前一个状态
		/// </summary>
		private State<_TyOwner> m_previous_state;


		/// <summary>
		/// 默认是初始状态
		/// </summary>
		private State<_TyOwner> m_default_start_state;

		/// <summary>
		/// 状态表
		/// </summary>
		private readonly Dictionary<String, State<_TyOwner>> m_states = new Dictionary<String, State<_TyOwner>>();

		#region Getter & Setter
		/// <summary>
		/// 获取当前状态
		/// </summary>
		/// <returns></returns>
		public State<_TyOwner> GetCurrentState() { return m_current_state; }

		/// <summary>
		/// 获取前一个状态
		/// </summary>
		/// <returns></returns>
		public State<_TyOwner> GetPrevState() { return m_previous_state; }

		/// <summary>
		/// 是否运行
		/// </summary>
		/// <returns></returns>
		public Boolean GetRunning() { return m_running; }
		#endregion

		/// <summary>
		/// 初始化
		/// </summary>
		public void Initialize()
		{
			InitializeState();
			if( m_states.Count == 0 )
				throw new Exception( "[StateMachine.Initialize]: StateMachine " );
			if( m_default_start_state == null )
				throw new Exception( "[StateMachine.Initialize]: Default State is null, use first one instead!" );
		}

		/// <summary>
		/// 初始化状态，必须继承
		/// </summary>
		protected abstract void InitializeState();

		public abstract _TyOwner GetOwner();

		/// <summary>
		/// 启动状态机
		/// </summary>
		public void Start()
		{
			m_running = true;
			if( m_previous_state != null || m_current_state != null )
				throw new Exception( "[StateMachine.Start] State Machine is already runing!" );

			m_current_state = m_default_start_state;
			m_current_state.OnEnter();
		}

		/// <summary>
		/// 停止状态机
		/// </summary>
		public void Stop()
		{
			if( m_current_state == null )
				throw new Exception( "[StateMachine.Stop] State Machine is not runing!" );
			m_running = false;
			m_previous_state = null;
			m_current_state.OnExit();
			m_current_state = null;
		}

		/// <summary>
		/// 状态转移
		/// </summary>
		/// <param name="_state_name"></param>
		public void Translate( String _state_name )
		{
			if( !m_running )
				throw new Exception( "[StateMachine.Translate] The state machine is not runing!" );

			State<_TyOwner> state;
			if( !m_states.TryGetValue( _state_name, out state ) )
				throw new Exception( string.Format( "[StateMachine.Translate] state [{0}] not found!", _state_name ) );

			if( state == m_current_state )
				throw new Exception( string.Format( "[StateMachine.Translate] state [{0}] Can't Translate to Self!", _state_name ) );


            UnityEngine.Debug.LogFormat( "[StateMachine.Translate] Exit from state: [{0}]", m_current_state.GetName() );
			m_current_state.OnExit();
			m_previous_state = m_current_state;
			m_current_state = state;
			UnityEngine.Debug.LogFormat( "[StateMachine.Translate] Enter state: [{0}]", m_current_state.GetName() );
			m_current_state.OnEnter();
		}

		/// <summary>
		/// 帧更新，这里的帧更新名字和U3D的规则相同
		/// </summary>
		public void Update()
		{
			if( !m_running )
				return;
			if( m_current_state.EnableUpdate )
				m_current_state.OnUpdate();
		}

		protected void AddState<_TyState>( String _name, Boolean _default ) where _TyState : State<_TyOwner>, new()
		{
			var state = (State<_TyOwner>)Activator.CreateInstance(typeof(_TyState), new object[] { GetOwner(), this, _name, });
			if( m_states.ContainsKey( _name ) )
				throw new Exception( string.Format( "[StateMachine.addState] State [{0}] Exsit!", _name ) );

			m_states.Add( _name, state );
			state.OnInit();
			if( _default )
			{
				if( m_default_start_state != null )
					throw new Exception( string.Format( "[StateMachine.addState] Default State repeat! old: {0} new: {1}", m_default_start_state.GetName(), state.GetName() ) );
				m_default_start_state = state;
			}
		}
	}

	/// <summary>
	/// 状态机状态 
	/// </summary>
	/// <typeparam name="_TyOwner"></typeparam>
	internal class State<_TyOwner> where _TyOwner : class, new()
	{
		protected String Name;
		protected StateMachine<_TyOwner> Fsm ;
		protected _TyOwner Owner;

		/// <summary>
		/// 是否开启Update
		/// </summary>
		public Boolean EnableUpdate = false;

		#region Getter & Setter
		public String GetName() { return Name; }
		#endregion

		public State() { }

		public State( _TyOwner _owner, StateMachine<_TyOwner> _fsm, String _name )
		{
			Owner = _owner;
			Name = _name;
			Fsm = _fsm;
		}

		public virtual void OnInit() { }
		public virtual void OnEnter() { }
		public virtual void OnUpdate() { }
		public virtual void OnExit() { }
	}
}
