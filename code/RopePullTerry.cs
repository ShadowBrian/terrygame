using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace terrygame
{
	public partial class RopePullTerry : AnimEntity
	{
		[Net]
		public int side { get; set; }

		[Net]
		public int direction { get; set; }

		[Net]
		public Vector3 RopePos { get; set; }

		[Net]
		public float RopeMovement { get; set; }

		public bool Grounded;

		float startheight;

		float smoothHeight;

		[Net]
		public AnimEntity TerryVisual { get; set; }

		[Net, Predicted]
		public SquidPlayer AssignedPlayer { get; set; }

		[Net]
		public Clothing.Container clothes { get; set; }

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/terry_spot_trigger.vmdl" );
			TerryVisual = new AnimEntity();
			TerryVisual.SetModel( "models/citizen/citizen.vmdl" );
			TerryVisual.SetAnimBool( "b_vr", true );
			TerryVisual.SetAnimFloat( "duck", 0.7f );

			TerryVisual.Position = Position;
			TerryVisual.Rotation = Rotation;

			TerryVisual.EnableDrawing = false;

			startheight = Position.z;

			smoothHeight = startheight;

			SetupPhysicsFromModel( PhysicsMotionType.Static );
			CollisionGroup = CollisionGroup.Trigger;
			EnableSolidCollisions = false;
			EnableTouch = true;
		}

		public override void StartTouch( Entity other )
		{
			if ( other is SquidPlayer && !(other as SquidPlayer).hasNumber )
			{
				AssignedPlayer = (other as SquidPlayer);

				this.Owner = other.Owner;

				//EnableTouch = false;
				EnableDrawing = false;
				TerryVisual.EnableDrawing = true;
				AssignedPlayer.hasNumber = true;
				other.EnableDrawing = false;

				CollisionGroup = CollisionGroup.Never;

				oldcontroller = AssignedPlayer.Controller;

				AssignedPlayer.Controller = null;

				if ( (other as SquidPlayer).TerryPuppet != null )
				{
					foreach ( var child in AssignedPlayer.TerryPuppet.Children )
					{
						if ( child is ModelEntity e )
						{
							var model = e.GetModelName();
							if ( model != null && !model.Contains( "clothes" ) && !model.Contains( "hair" ) && !model.Contains( "hat" ) ) // Uck we 're better than this, entity tags, entity type or something?
								continue;

							var clothing = new ModelEntity();
							clothing.SetModel( model );
							clothing.SetParent( TerryVisual, true );

							clothesCopied.Add( clothing );
						}
					}
					AssignedPlayer.TerryPuppet.EnableDrawing = false;

					clothes = (other as SquidPlayer).clothes;
					clothes.DressEntity( TerryVisual );
				}
				else
				{
					foreach ( var child in other.Children )
					{
						if ( child is ModelEntity e )
						{
							var model = e.GetModelName();
							if ( model != null && !model.Contains( "clothes" ) && !model.Contains( "hair" ) && !model.Contains( "hat" ) ) // Uck we 're better than this, entity tags, entity type or something?
								continue;

							var clothing = new ModelEntity();
							clothing.SetModel( model );
							clothing.SetParent( TerryVisual, true );

							clothesCopied.Add( clothing );
						}
					}

					clothes = (other as SquidPlayer).clothes;

					clothes.DressEntity( TerryVisual );

				}
			}

			base.Touch( other );

		}

		float Accuracy;
		bool CheckingInput;
		bool FlipValue;

		[Event.Tick.Client]
		public void TickClient()
		{
			if(AssignedPlayer != null )
			{
				AssignedPlayer.Position = Position;

				DebugOverlay.Text( Position, "Accuracy: " + Accuracy);
				if ( CheckingInput )
				{
					Accuracy += Time.Delta * 100f * (FlipValue?-1:1);
					if(Accuracy >= 100f )
					{
						FlipValue = true;
					}

					if(Accuracy <= 0f )
					{
						FlipValue = false;
					}
				}

				if ( Input.Pressed( InputButton.Jump ) )
				{
					CheckingInput = !CheckingInput;
				}

				if ( Input.Pressed( InputButton.Menu ) )
				{
					//AssignedPlayer.EnableDrawing = true;
					//AssignedPlayer.Controller = new WalkController();
					//AssignedPlayer.Spawn();
					//UndoAssignedPlayer();
					ClearAssignedPlayer( this.NetworkIdent );
				}
			}
		}

		List<Entity> clothesCopied = new List<Entity>();

		PawnController oldcontroller;

		public void UndoAssignedPlayer()
		{
			//EnableTouch = true;
			EnableDrawing = true;
			TerryVisual.EnableDrawing = false;
			AssignedPlayer.hasNumber = false;
			AssignedPlayer.EnableDrawing = true;
			AssignedPlayer.Controller = oldcontroller;

			if ( AssignedPlayer.TerryPuppet != null )
			{
				foreach ( var child in AssignedPlayer.TerryPuppet.Children )
				{
					if ( child is ModelEntity e )
					{
						//var model = e.GetModelName();
						//if ( model != null && !model.Contains( "clothes" ) ) // Uck we 're better than this, entity tags, entity type or something?
						//continue;

						e.EnableDrawing = true;
					}
				}
				AssignedPlayer.TerryPuppet.EnableDrawing = true;
			}
			else
			{
				foreach ( var child in AssignedPlayer.Children )
				{
					if ( child is ModelEntity e )
					{
						//var model = e.GetModelName();
						//if ( model != null && !model.Contains( "clothes" ) ) // Uck we 're better than this, entity tags, entity type or something?
						//continue;

						e.EnableDrawing = true;
					}
				}
			}

			for ( int i = 0; i < clothesCopied.Count; i++ )
			{
				clothesCopied[i].Delete();
			}
			clothesCopied.Clear();

			clothes.ClearEntities();

			clothes.DressEntity( AssignedPlayer );

			AssignedPlayer = null;

			//Task.DelaySeconds( 1f );

			//EnableTouch = true;
		}

		public Task ReEnableCollision;

		[ServerCmd]
		public static void ClearAssignedPlayer( int entname )
		{
			(Entity.FindByIndex( entname ) as RopePullTerry).UndoAssignedPlayer();
		}

		[Event.Tick.Server]
		public void TickServer( )
		{
		
			if(!AssignedPlayer.IsValid() && TerryVisual.EnableDrawing)
			{
				Log.Trace( "Unassigned the player!" );
				UndoAssignedPlayer( );
			}

			if ( AssignedPlayer != null )
			{
				AssignedPlayer.Position = Position;
			}

			TerryVisual.SetAnimFloat( "move_x", RopeMovement * 15f);

			Trace floortrace = Trace.Ray( Position + Vector3.Up * 31f, Position - Vector3.Up );
			floortrace.UseHitboxes( false );

			TraceResult result = floortrace.Run();
			Grounded = result.Hit;

			TerryVisual.SetAnimBool( "b_grounded", Grounded );

			TerryVisual.Position = Position;
			TerryVisual.Rotation = Rotation;

			if ( !Grounded )
			{
				smoothHeight = MathX.LerpTo( smoothHeight, startheight - 20f, 0.5f );
			}
			else
			{
				smoothHeight = MathX.LerpTo( smoothHeight, startheight, 0.5f );
			}

			Position = Position.WithZ( smoothHeight );

			if (direction == -1 )
			{
				if ( side == -1 )
				{
					TerryVisual.SetAnimVector( "right_hand_ik.position", (Rotation.Right * 10f * side) + Rotation.Forward * ((17f + RopeMovement * 3f) * direction) + Rotation.Up * (30f + (20f*(result.Hit?0:1))) );

					TerryVisual.SetAnimVector( "left_hand_ik.position", (Rotation.Right * 0f * side) + Rotation.Forward * ((15f + RopeMovement * 3f) * direction) + Rotation.Up * (30f + (20f * (result.Hit ? 0 : 1)) ) );

					TerryVisual.SetAnimRotation( "right_hand_ik.rotation", Rotation * Rotation.From( new Angles( 90, 0, 90 ) ) * Rotation.From( new Angles( 180, 180, 0 ) ) );

					TerryVisual.SetAnimRotation( "left_hand_ik.rotation", Rotation * Rotation.From( new Angles( 120, -65, 45 ) ) * Rotation.From( new Angles( 180, 180, 0 ) ) );
				}
				else
				{
					TerryVisual.SetAnimVector( "left_hand_ik.position", (Rotation.Right * 10f * side) + Rotation.Forward * ((17f + RopeMovement * 3f) * direction) + Rotation.Up * (30f + (20f * (result.Hit ? 0 : 1)) ) );

					TerryVisual.SetAnimVector( "right_hand_ik.position", (Rotation.Right * 1f * side) + Rotation.Forward * ((15f + RopeMovement * 3f) * direction) + Rotation.Up * (30f + (20f * (result.Hit ? 0 : 1))) );

					TerryVisual.SetAnimRotation( "left_hand_ik.rotation", Rotation * Rotation.From( new Angles( 75, 0, 90 ) ) * Rotation.From( new Angles( 180, 180, 0 ) ));

					TerryVisual.SetAnimRotation( "right_hand_ik.rotation", Rotation * Rotation.From( new Angles( 75, -120, -45 ) ) * Rotation.From( new Angles( 180, 180, 0 ) ));
				}
			}
			else
			{
				if ( side == -1 )
				{
					TerryVisual.SetAnimVector( "left_hand_ik.position", (Rotation.Right * 10f * side) + Rotation.Forward * ((17f - RopeMovement * 3f) * direction) + Rotation.Up * (30f + (20f * (result.Hit ? 0 : 1))) );

					TerryVisual.SetAnimVector( "right_hand_ik.position", (Rotation.Right * 3f * side) + Rotation.Forward * ((15f - RopeMovement * 3f) * direction) + Rotation.Up * (30f + (20f * (result.Hit ? 0 : 1))) );

					TerryVisual.SetAnimRotation( "left_hand_ik.rotation", Rotation * Rotation.From( new Angles( 90, 0, 90 ) ) );

					TerryVisual.SetAnimRotation( "right_hand_ik.rotation", Rotation * Rotation.From( new Angles( 120, -65, 45 ) ) );
				}
				else
				{
					TerryVisual.SetAnimVector( "right_hand_ik.position", (Rotation.Right * 10f * side) + Rotation.Forward * ((17f - RopeMovement * 3f) * direction) + Rotation.Up * (30f + (20f * (result.Hit ? 0 : 1))) );

					TerryVisual.SetAnimVector( "left_hand_ik.position", (Rotation.Right * 1f * side) + Rotation.Forward * ((15f - RopeMovement * 3f) * direction) + Rotation.Up * (30f + (20f * (result.Hit ? 0 : 1))) );

					TerryVisual.SetAnimRotation( "right_hand_ik.rotation", Rotation * Rotation.From( new Angles( 75, 0, 90 ) ) );

					TerryVisual.SetAnimRotation( "left_hand_ik.rotation", Rotation * Rotation.From( new Angles( 75, -120, -45 ) ) );
				}
			}
		}
	}
}
