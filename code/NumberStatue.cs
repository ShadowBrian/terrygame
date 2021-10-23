using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
namespace terrygame
{
	partial class NumberStatue : ModelEntity
	{
		[Net]
		public int NumberToShow { get; set; }

		[Net]
		public ModelEntity numberPlate { get; set; }

		[Net]
		bool NumberTaken { get; set; }

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/glassbridge_numberholdstatue.vmdl" );

			numberPlate = new ModelEntity();
			numberPlate.SetModel( "models/clothes/glassbridge_numberjacket.vmdl" );
			//numberPlate.SetParent( this );
			numberPlate.Position = Position - Vector3.Up * 25f;
			numberPlate.Rotation = Rotation;

			UsePhysicsCollision = true;
			EnableDrawing = true;

			EnableTouch = true;

			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		}

		[Event.Tick]
		void Tick()
		{
			if ( IsClient )
			{
				if ( numberPlate != null && numberPlate.SceneObject != null )
				{
					numberPlate.SceneObject.SetValue( "pnum", NumberToShow );
				}
			}
		}

		public override void Touch( Entity other )
		{
			if (other is SquidPlayer && !NumberTaken && !(other as SquidPlayer).hasNumber )
			{
				if ( (other as SquidPlayer).TerryPuppet != null )
				{
					numberPlate.SetParent( (other as SquidPlayer).TerryPuppet, true );
				}
				else
				{
					numberPlate.SetParent( other, true );
				}
				NumberTaken = true;
				(other as SquidPlayer).hasNumber = true;

				//Delete();
			}

			base.Touch( other );
			
		}
	}
}
