using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	public partial class JumpPad_Trigger : GameTypeBase
	{
		public float JumpForce;

		[Net]
		public JumpPad AssignedPad { get; set; }

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/jumppad_trigger.vmdl" );

			SetupPhysicsFromModel( PhysicsMotionType.Static );

			CollisionGroup = CollisionGroup.Trigger;
			EnableSolidCollisions = true;
			EnableTouch = true;
		}

		public override void Touch( Entity other )
		{
			if ( other is SquidPlayer && IsServer)
			{
				SquidPlayer player = other as SquidPlayer;
				player.GroundEntity = null;
				player.Position += Vector3.Up * 2f;

				player.Velocity = player.Velocity.WithZ(JumpForce);

				//Log.Trace( JumpForce );

				Sound.FromEntity( "boing", this );

				AssignedPad.DoJump();
			}
		}
	}
}
