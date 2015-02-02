﻿using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Sandbox.Common.ObjectBuilders;
using SEModAPIInternal.API.Common;
using SEModAPIInternal.Support;

namespace SEModAPIInternal.API.Entity.Sector.SectorObject.CubeGrid.CubeBlock
{
	[DataContract( Name = "TurretBaseEntityProxy" )]
	public class TurretBaseEntity : FunctionalBlockEntity
	{
		#region "Attributes"

		private TurretNetworkManager m_turretNetworkManager;

		private InventoryEntity m_inventory;
		private BaseObject m_target;
		private float m_shootingRange;
		private float m_searchingRange;
		private bool m_targetMeteors;
		private bool m_targetMoving;
		private bool m_targetMissiles;

		public static string TurretNamespace = "Sandbox.Game.Weapons";
		public static string TurretClass = "MyLargeTurretBase";

		public static string TurretIsTargetMethod = "IsTarget";
		public static string TurretIsTargetVisibleMethod = "IsTargetVisible";
		public static string TurretIsTargetInViewMethod = "IsTargetInView";
		public static string TurretIsTargetEnemyMethod = "IsTargetEnemy";
		public static string TurretGetNearestVisibleTargetMethod = "GetNearestVisibleTarget";
		public static string TurretShootMethod = "Shoot";
		public static string TurretGetRemainingAmmoMethod = "GetAmmunitionAmount";

		public static string TurretSearchingRangeField = "m_searchingRange";
		public static string TurretInventoryField = "m_ammoInventory";

		public static string TurretTargetProperty = "Target";
		public static string TurretShootingRangeProperty = "ShootingRange";
		public static string TurretTargetMeteorsProperty = "TargetMeteors";
		public static string TurretTargetMissilesProperty = "TargetMissiles";
		public static string TurretTargetMovingProperty = "TargetMoving";
		public static string TurretNetworkManagerProperty = "SyncObject";

		#endregion "Attributes"

		#region "Constructors and Intializers"

		public TurretBaseEntity( CubeGridEntity parent, MyObjectBuilder_TurretBase definition )
			: base( parent, definition )
		{
			m_inventory = new InventoryEntity( definition.Inventory );

			m_shootingRange = definition.Range;
			m_searchingRange = m_shootingRange + 100;

			m_targetMeteors = definition.TargetMeteors;
			m_targetMissiles = definition.TargetMissiles;
			m_targetMoving = definition.TargetMoving;
		}

		public TurretBaseEntity( CubeGridEntity parent, MyObjectBuilder_TurretBase definition, Object backingObject )
			: base( parent, definition, backingObject )
		{
			m_turretNetworkManager = new TurretNetworkManager( this, GetNetworkManager( ) );
			m_inventory = new InventoryEntity( definition.Inventory, GetTurretInventory( ) );

			m_shootingRange = definition.Range;
			m_searchingRange = m_shootingRange + 100;

			m_targetMeteors = definition.TargetMeteors;
			m_targetMissiles = definition.TargetMissiles;
			m_targetMoving = definition.TargetMoving;
		}

		#endregion "Constructors and Intializers"

		#region "Properties"

		[IgnoreDataMember]
		[Category( "Turret" )]
		[Browsable( false )]
		new internal MyObjectBuilder_TurretBase ObjectBuilder
		{
			get { return (MyObjectBuilder_TurretBase)base.ObjectBuilder; }
			set
			{
				base.ObjectBuilder = value;
			}
		}

		[DataMember]
		[Category( "Turret" )]
		public float ShootingRange
		{
			get
			{
				if ( BackingObject == null || ActualObject == null )
					return ObjectBuilder.Range;

				float range = GetShootingRange( );
				return range;
			}
			set
			{
				m_shootingRange = value;

				Action action = SetShootingRange;
				SandboxGameAssemblyWrapper.Instance.EnqueueMainGameAction( action );
			}
		}

		[DataMember]
		[Category( "Turret" )]
		public float SearchingRange
		{
			get
			{
				if ( BackingObject == null || ActualObject == null )
					return ObjectBuilder.Range;

				float range = GetSearchingRange( );
				return range;
			}
			set
			{
				m_searchingRange = value;

				Action action = SetSearchingRange;
				SandboxGameAssemblyWrapper.Instance.EnqueueMainGameAction( action );
			}
		}

		[IgnoreDataMember]
		[Category( "Turret" )]
		public BaseObject Target
		{
			get
			{
				try
				{
					if ( BackingObject == null || ActualObject == null )
						return m_target;

					Object target = GetTarget( );
					if ( target == null )
						return null;

					if ( m_target == null )
					{
						long entityId = BaseEntity.GetEntityId( target );
						m_target = GameEntityManager.GetEntity( entityId );
					}

					return m_target;
				}
				catch ( Exception ex )
				{
					LogManager.ErrorLog.WriteLine( ex );
					return m_target;
				}
			}
			set
			{
				m_target = value;

				Action action = SetTarget;
				SandboxGameAssemblyWrapper.Instance.EnqueueMainGameAction( action );
			}
		}

		[DataMember]
		[Category( "Turret" )]
		[ReadOnly( true )]
		public int RemainingAmmo
		{
			get
			{
				if ( BackingObject == null || ActualObject == null )
					return ObjectBuilder.RemainingAmmo;

				int ammo = GetRemainingAmmo( );
				return ammo;
			}
			private set
			{
				//Do nothing!
			}
		}

		[DataMember]
		[Category( "Turret" )]
		public bool TargetMeteors
		{
			get
			{
				if ( BackingObject == null || ActualObject == null )
					return ObjectBuilder.TargetMeteors;

				return GetTargetMeteors( );
			}
			set
			{
				ObjectBuilder.TargetMeteors = value;
				m_targetMeteors = value;

				SetTargetMeteors( );
			}
		}

		[DataMember]
		[Category( "Turret" )]
		public bool TargetMissiles
		{
			get
			{
				if ( BackingObject == null || ActualObject == null )
					return ObjectBuilder.TargetMissiles;

				return GetTargetMissiles( );
			}
			set
			{
				ObjectBuilder.TargetMissiles = value;
				m_targetMissiles = value;

				SetTargetMissiles( );
			}
		}

		[DataMember]
		[Category( "Turret" )]
		public bool TargetMoving
		{
			get
			{
				if ( BackingObject == null || ActualObject == null )
					return ObjectBuilder.TargetMoving;

				return GetTargetMoving( );
			}
			set
			{
				m_targetMoving = value;

				SetTargetMoving( );
			}
		}

		[DataMember]
		[Category( "Turret" )]
		[ReadOnly( true )]
		public long NearestTargetId
		{
			get
			{
				if ( BackingObject == null || ActualObject == null )
					return 0;

				long id = GetNearestTargetId( );
				return id;
			}
			private set
			{
				//Do nothing!
			}
		}

		[IgnoreDataMember]
		[Category( "Turret" )]
		[Browsable( false )]
		public InventoryEntity Inventory
		{
			get { return m_inventory; }
		}

		[IgnoreDataMember]
		[Category( "Turret" )]
		[Browsable( false )]
		internal TurretNetworkManager TurretNetManager
		{
			get { return m_turretNetworkManager; }
		}

		#endregion "Properties"

		#region "Methods"

		new public static bool ReflectionUnitTest( )
		{
			try
			{
				bool result = true;

				Type type = SandboxGameAssemblyWrapper.Instance.GetAssemblyType( TurretNamespace, TurretClass );
				if ( type == null )
					throw new Exception( "Could not find internal type for TurretBaseEntity" );

				result &= HasMethod( type, TurretIsTargetMethod );
				result &= HasMethod( type, TurretIsTargetVisibleMethod );
				result &= HasMethod( type, TurretIsTargetInViewMethod );
				result &= HasMethod( type, TurretIsTargetEnemyMethod );
				result &= HasMethod( type, TurretGetNearestVisibleTargetMethod );
				result &= HasMethod( type, TurretGetRemainingAmmoMethod );

				result &= HasField( type, TurretSearchingRangeField );
				result &= HasField( type, TurretInventoryField );

				result &= HasProperty( type, TurretTargetProperty );
				result &= HasProperty( type, TurretShootingRangeProperty );
				result &= HasProperty( type, TurretTargetMeteorsProperty );
				result &= HasProperty( type, TurretTargetMissilesProperty );
				result &= HasProperty( type, TurretTargetMovingProperty );
				result &= HasProperty( type, TurretNetworkManagerProperty );

				return result;
			}
			catch ( Exception ex )
			{
				Console.WriteLine( ex );
				return false;
			}
		}

		public bool IsTarget( BaseObject gameEntity )
		{
			bool result = (bool)InvokeEntityMethod( ActualObject, TurretIsTargetMethod, new object[ ] { gameEntity.BackingObject } );
			return result;
		}

		public bool IsTargetVisible( BaseObject gameEntity )
		{
			bool result = (bool)InvokeEntityMethod( ActualObject, TurretIsTargetVisibleMethod, new object[ ] { gameEntity.BackingObject } );
			return result;
		}

		public bool IsTargetInView( BaseObject gameEntity )
		{
			bool result = (bool)InvokeEntityMethod( ActualObject, TurretIsTargetInViewMethod, new object[ ] { gameEntity.BackingObject } );
			return result;
		}

		public bool IsTargetEnemy( BaseObject gameEntity )
		{
			bool result = (bool)InvokeEntityMethod( ActualObject, TurretIsTargetEnemyMethod, new object[ ] { gameEntity.BackingObject } );
			return result;
		}

		public long GetNearestTargetId( )
		{
			try
			{
				Object nearestTarget = GetNearestVisibleTarget( );
				if ( nearestTarget == null )
					return 0;

				long result = BaseEntity.GetEntityId( nearestTarget );
				return result;
			}
			catch ( Exception ex )
			{
				LogManager.ErrorLog.WriteLine( ex );
				return 0;
			}
		}

		public void Shoot( )
		{
			Action action = InternalTurretShoot;
			SandboxGameAssemblyWrapper.Instance.EnqueueMainGameAction( action );
		}

		protected Object GetNetworkManager( )
		{
			//Object result = GetEntityFieldValue(ActualObject, TurretNetworkManagerField);
			Object result = GetEntityProperty( ActualObject, TurretNetworkManagerProperty );
			return result;
		}

		protected Object GetNearestVisibleTarget( )
		{
			try
			{
				Object result = InvokeEntityMethod( ActualObject, TurretGetNearestVisibleTargetMethod, new object[ ] { m_searchingRange, false } );
				return result;
			}
			catch ( Exception ex )
			{
				LogManager.ErrorLog.WriteLine( ex );
				return null;
			}
		}

		protected int GetRemainingAmmo( )
		{
			int result = (int)InvokeEntityMethod( ActualObject, TurretGetRemainingAmmoMethod );
			return result;
		}

		protected Object GetTarget( )
		{
			Object result = GetEntityPropertyValue( ActualObject, TurretTargetProperty );
			return result;
		}

		protected void SetTarget( )
		{
			if ( m_target == null )
				return;

			SetEntityPropertyValue( ActualObject, TurretTargetProperty, m_target.BackingObject );

			TurretNetManager.BroadcastTargetId( );
		}

		protected float GetShootingRange( )
		{
			float result = (float)GetEntityPropertyValue( ActualObject, TurretShootingRangeProperty );
			return result;
		}

		protected void SetShootingRange( )
		{
			SetEntityPropertyValue( ActualObject, TurretShootingRangeProperty, m_shootingRange );

			TurretNetManager.BroadcastRange( );
		}

		protected float GetSearchingRange( )
		{
			try
			{
				float result = (float)GetEntityFieldValue( ActualObject, TurretSearchingRangeField );
				return result;
			}
			catch ( Exception ex )
			{
				LogManager.ErrorLog.WriteLine( ex );
				return m_searchingRange;
			}
		}

		protected void SetSearchingRange( )
		{
			SetEntityFieldValue( ActualObject, TurretSearchingRangeField, m_searchingRange );
		}

		protected bool GetTargetMeteors( )
		{
			bool result = (bool)GetEntityPropertyValue( ActualObject, TurretTargetMeteorsProperty );
			return result;
		}

		protected void SetTargetMeteors( )
		{
			SetEntityPropertyValue( ActualObject, TurretTargetMeteorsProperty, m_targetMeteors );

			TurretNetManager.BroadcastTargettingFlags( );
		}

		protected bool GetTargetMissiles( )
		{
			bool result = (bool)GetEntityPropertyValue( ActualObject, TurretTargetMissilesProperty );
			return result;
		}

		protected void SetTargetMissiles( )
		{
			SetEntityPropertyValue( ActualObject, TurretTargetMissilesProperty, m_targetMissiles );

			TurretNetManager.BroadcastTargettingFlags( );
		}

		protected bool GetTargetMoving( )
		{
			bool result = (bool)GetEntityPropertyValue( ActualObject, TurretTargetMovingProperty );
			return result;
		}

		protected void SetTargetMoving( )
		{
			SetEntityPropertyValue( ActualObject, TurretTargetMovingProperty, m_targetMoving );

			TurretNetManager.BroadcastTargettingFlags( );
		}

		protected Object GetTurretInventory( )
		{
			Object result = GetEntityFieldValue( ActualObject, TurretInventoryField );
			return result;
		}

		protected void InternalTurretShoot( )
		{
			InvokeEntityMethod( ActualObject, TurretShootMethod );
		}

		#endregion "Methods"
	}
}