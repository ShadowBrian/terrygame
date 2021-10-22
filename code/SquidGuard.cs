using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	[Library( "terrygame_squidguard", Description = "Squid Guard NPC" )]
	[Hammer.EditorModel( "models/clothes/squidgameguardsuit.vmdl" )]
	class SquidGuard : AnimEntity
	{
		ModelEntity suit, helmet, feet;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/citizen/citizen.vmdl" );

			suit = new ModelEntity();
			suit.SetModel( "models/clothes/squidgameguardsuit.vmdl" );
			suit.SetParent( this, true );
			suit.EnableShadowInFirstPerson = true;
			suit.EnableHideInFirstPerson = true;

			helmet = new ModelEntity();
			helmet.SetModel( "models/clothes/squidgameguardmask.vmdl" );
			helmet.SetParent( this, true );
			helmet.EnableShadowInFirstPerson = true;
			helmet.EnableHideInFirstPerson = true;

			feet = new ModelEntity();
			feet.SetModel( "models/citizen_clothes/shoes/shoes.police.vmdl" );
			feet.SetParent( this, true );
			feet.EnableShadowInFirstPerson = true;
			feet.EnableHideInFirstPerson = true;

			SetBodyGroup( 0, 0 );
			SetBodyGroup( 1, 1 );
			SetBodyGroup( 2, 1 );
			SetBodyGroup( 3, 0 );
			SetBodyGroup( 4, 1 );

			UsePhysicsCollision = true;
		}
	}
}
