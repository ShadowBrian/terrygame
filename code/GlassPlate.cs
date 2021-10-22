using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
namespace terrygame
{
	partial class GlassPlate : ModelEntity
	{
		public bool breakable;
		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/glassplate.vmdl" );
			UsePhysicsCollision = true;
			EnableDrawing = true;
			Scale = 2.5f;

			EnableTouch = true;

			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		}

		public override void Touch( Entity other )
		{
			if (other is SquidPlayer && breakable )
			{
				Delete();
			}

			base.Touch( other );
			
		}
	}
}
