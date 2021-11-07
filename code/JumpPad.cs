using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	[Library( "terrygame_jumppad", Description = "Jump Pad" )]
	[Hammer.EditorModel( "models/jumppad.vmdl" )]
	public partial class JumpPad : GameTypeBase
	{
		[Property( Title = "Jump Force" )]
		public float JumpForce
		{
			get;
			set;
		}

		[Net]
		JumpPad_Trigger assignedTrigger { get; set; }

		public async Task DoJump()
		{

			SetAnimBool( "Jump", true );

			await Task.DelaySeconds( 0.1f );

			SetAnimBool( "Jump", false );
		}

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/jumppad.vmdl" );

			SetupPhysicsFromModel( PhysicsMotionType.Static );

			CollisionGroup = CollisionGroup.Player;

			EnableSolidCollisions = true;

			SetAnimBool( "Jump", false );

			assignedTrigger = new JumpPad_Trigger();
			assignedTrigger.Transform = Transform;
			assignedTrigger.Scale = Scale;
			assignedTrigger.JumpForce = JumpForce;
			assignedTrigger.AssignedPad = this;
		}

	}
}
