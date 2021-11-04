using Sandbox;

namespace terrygame
{
	partial class SquidPlayer
	{
		// TODO - make ragdolls one per entity
		// TODO - make ragdolls dissapear after a load of seconds
		//static EntityLimit RagdollLimit = new EntityLimit { MaxTotal = 20 };

		[ClientRpc]
		void BecomeRagdollOnClient( Vector3 force, int forceBone, ModelEntity puppet = null, bool usepuppet = false)
		{
			// TODO - lets not make everyone write this shit out all the time
			// maybe a CreateRagdoll<T>() on ModelEntity?
			var ent = new ModelEntity();
			ent.Position = Position;
			ent.Rotation = Rotation;
			ent.MoveType = MoveType.Physics;
			ent.UsePhysicsCollision = true;
			ent.SetInteractsAs( CollisionLayer.Debris );
			ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
			ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

			ent.SetModel( GetModelName() );
			ent.CopyBonesFrom( usepuppet ? puppet : this );
			ent.TakeDecalsFrom( usepuppet ? puppet : this );
			ent.SetRagdollVelocityFrom( this );

			ent.SetBodyGroup( 0, 0 );
			ent.SetBodyGroup( 1, 1 );
			ent.SetBodyGroup( 2, 1 );
			ent.SetBodyGroup( 3, 0 );
			ent.SetBodyGroup( 4, 1 );
			//ent.DeleteAsync( 20.0f );

			// Copy the clothes over
			foreach ( var child in usepuppet ? puppet.Children : Children )
			{
				if ( child is ModelEntity e )
				{
					var model = e.GetModelName();
					if ( model != null && !model.Contains( "clothes" ) ) // Uck we 're better than this, entity tags, entity type or something?
						continue;

					var clothing = new ModelEntity();
					clothing.SetModel( model );
					clothing.SetParent( ent, true );
					e.EnableDrawing = false;
				}
			}

			ent.PhysicsGroup.AddVelocity( force );

			if ( forceBone >= 0 )
			{
				var body = ent.GetBonePhysicsBody( forceBone );
				if ( body != null )
				{
					body.ApplyForce( force * 1000 );
				}
				else
				{
					ent.PhysicsGroup.AddVelocity( force );
				}
			}


			Corpse = ent;

			//RagdollLimit.Watch( ent );
		}
	}
}
