using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;


namespace terrygame
{
	public partial class SquidPlayer : Player
	{
		[Net]
		ModelEntity suit { get; set; }
		ModelEntity feet, bottom, hat;

		[Net]
		public Clothing.Container clothes { get; set; }

		public Vector3 headpos;

		[Net]
		public bool hasNumber { get; set; }

		[Net, Predicted]
		AnimEntity LH { get; set; }

		[Net, Predicted]
		AnimEntity RH { get; set; }

		[Net, Predicted]
		public TerryPup TerryPuppet { get; set; }

		[Net]
		public int PlayerNum { get; set; }

		public TerryGame.GameTypes gametype;

		public bool moving;

		[Net]
		WalkControllerVR ControllerRef { get; set; }

		[Net]
		StandardPlayerAnimatorVR AnimatorRef { get; set; }

		PushAbility pushAbility;

		Color[] TeamColors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Yellow };

		[Net]
		public bool TeamBased { get; set; }

		[Net]
		public int TeamsCount { get; set; }

		[Net, Predicted]
		public AnimEntity Bomb { get; set; }

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );			

			gametype = (Game.Current as TerryGame).gameType;

			//Log.Trace( gametype );

			EnableTouch = true;

			if ( Input.VR.IsActive )
			{
				if ( Animator == null )
				{
					if ( gametype == TerryGame.GameTypes.RGLight )
					{
						Controller = new WalkControllerVRRGLight();
						ControllerRef = Controller as WalkControllerVRRGLight;
					}
					else
					{
						Controller = new WalkControllerVR();
						ControllerRef = Controller as WalkControllerVR;
					}

					ControllerRef.BodyGirth = 12f;

					Animator = new StandardPlayerAnimatorVR();

					AnimatorRef = Animator as StandardPlayerAnimatorVR;

					pushAbility = new PushAbility();
					pushAbility.SetParent( this );

					Camera = new FirstPersonCamera();
				}

				if(TerryPuppet != null )
				{
					TerryPuppet.EnableDrawing = true;
				}

				if(LH != null )
				{
					LH.EnableDrawing = true;
				}

				if ( RH != null )
				{
					RH.EnableDrawing = true;
				}
				EnableAllCollisions = true;
				EnableDrawing = true;
				EnableHideInFirstPerson = false;
				EnableShadowInFirstPerson = true;


			}
			else
			{
				if ( gametype == TerryGame.GameTypes.RGLight )
				{
					Controller = new WalkControllerFlatRGLight();
				}
				else if( gametype == TerryGame.GameTypes.GlassBridge )
				{
					Controller = new WalkControllerFlatNoRun();
				}
				else
				{
					Controller = new WalkControllerStairsfix();
				}

				Animator = new StandardPlayerAnimator();

				Camera = new ThirdPersonCamera();

				pushAbility = new PushAbility();
				pushAbility.SetParent( this );


				EnableAllCollisions = true;
				EnableDrawing = true;
				EnableHideInFirstPerson = true;
				EnableShadowInFirstPerson = true;

			}

			if ( suit == null )
			{
				/*suit = new ModelEntity();
				suit.SetModel( "models/clothes/christmassuit.vmdl" );
				suit.Tags.Add( "suit" );
				suit.SetParent( this, true );
				suit.EnableShadowInFirstPerson = true;
				suit.EnableHideInFirstPerson = true;*/

				suit = new ModelEntity();
				suit.SetModel( "models/clothes/tracksuit_top.vmdl" );
				suit.Tags.Add( "suit" );
				suit.SetParent( this, true );
				suit.EnableShadowInFirstPerson = true;
				suit.EnableHideInFirstPerson = true;

				if ( TeamBased )
				{
					suit.SetMaterialGroup( 1 );
					suit.RenderColor = (TeamColors[(PlayerNum - 1) % TeamsCount] * 0.6f).WithAlpha( 1f );
				}

				bottom = new ModelEntity();
				bottom.SetModel( "models/clothes/tracksuit_bottom.vmdl" );
				bottom.SetParent( this, true );
				bottom.EnableShadowInFirstPerson = true;
				bottom.EnableHideInFirstPerson = true;

				feet = new ModelEntity();
				feet.SetModel( "models/clothes/tracksuit_shoes.vmdl" );
				feet.SetParent( this, true );
				feet.EnableShadowInFirstPerson = true;
				feet.EnableHideInFirstPerson = true;

				/*ModelEntity glove = new ModelEntity();
				glove.SetModel( "models/clothes/bowlingglove_left.vmdl" );
				glove.SetParent( this, true );
				glove.EnableShadowInFirstPerson = true;
				glove.EnableHideInFirstPerson = true;

				glove = new ModelEntity();
				glove.SetModel( "models/clothes/bowlingglove_right.vmdl" );
				glove.SetParent( this, true );
				glove.EnableShadowInFirstPerson = true;
				glove.EnableHideInFirstPerson = true;*/

				//suit = new ModelEntity();
				//suit.SetModel( "models/clothes/glassbridge_numberjacket.vmdl" );
				//suit.Tags.Add( "suit" );
				//suit.SetParent( this, true );
				//suit.EnableShadowInFirstPerson = true;
				//suit.EnableHideInFirstPerson = true;

				clothes = new();

				clothes.LoadFromClient(Client);

				List<Clothing> yeetclothes = new List<Clothing>();

				for ( int i = 0; i < clothes.Clothing.Count; i++ )
				{
					if ( clothes.Clothing[i].Category == Clothing.ClothingCategory.Bottoms || clothes.Clothing[i].Category == Clothing.ClothingCategory.Footwear || clothes.Clothing[i].Category == Clothing.ClothingCategory.Tops )
					{
						yeetclothes.Add(clothes.Clothing[i]);
					}
				}

				foreach ( var item in yeetclothes )
				{
					clothes.Clothing.Remove( item );
				}

				if(!Died)
					clothes.DressEntity( this );

			}

			SetBodyGroup( 0, 0 );
			SetBodyGroup( 1, 1 );
			SetBodyGroup( 2, 1 );
			SetBodyGroup( 3, 0 );
			SetBodyGroup( 4, 1 );

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Health = 100;

			base.Respawn();

			if ( Died || (Game.Current as TerryGame).eliminatedPlayers.Contains( this.Client.Name.ToString() ) )
			{
				Died = true;
				Log.Trace( "Tried to spawn, but I am dead." );
				Eliminated();
				suit.EnableDrawing = false;
				feet.EnableDrawing = false;
				bottom.EnableDrawing = false;
			}
		}

		[ClientRpc]
		void ResetRotation()
		{
			if(AnimatorRef != null)
			AnimatorRef.PlayerRot = Rotation.Identity.Angles();
		}

		/// <summary>
		/// Called once the player's health reaches 0
		/// </summary>
		public override void OnKilled()
		{
			/*base.OnKilled();

			if ( Input.VR.IsActive )
			{
				BecomeRagdollOnClient( -Rotation.Forward * 100f, 0, TerryPuppet, true );
			}
			else
			{
				BecomeRagdollOnClient( -Rotation.Forward * 100f, 0 );
			}

			//ResetRotation();
			//if ( AnimatorRef != null )
				//AnimatorRef.PlayerRot = Rotation.Identity.Angles();

			//Controller = null;

			
			Camera = new SpectateRagdollCamera();

			EnableAllCollisions = false;
			EnableDrawing = false;

			if ( TerryPuppet != null )
			{
				TerryPuppet.EnableDrawing = false;
			}

			if ( LH != null )
			{
				LH.EnableDrawing = false;
			}

			if ( RH != null )
			{
				RH.EnableDrawing = false;
			}*/
			Eliminated();
			(Game.Current as TerryGame).AddEliminatedPlayer( Client.Name.ToString() );
		}

		public bool Died;

		public virtual void Eliminated()
		{
			EnableAllCollisions = false;
			EnableDrawing = false;

			if ( !Died )
			{
				if ( Input.VR.IsActive )
				{
					BecomeRagdollOnClient( -Rotation.Forward * 100f, 0, TerryPuppet, true );
				}
				else
				{
					BecomeRagdollOnClient( -Rotation.Forward * 100f, 0 );
				}
			}

			Camera = new FirstPersonCamera();

			if ( Input.VR.IsActive )
			{
				Controller = new NoclipControllerVR();
			}
			else
			{
				Controller = new NoclipController();
			}

			if ( TerryPuppet != null )
			{
				TerryPuppet.EnableDrawing = false;
			}

			if ( LH != null )
			{
				LH.EnableDrawing = false;
			}

			if ( RH != null )
			{
				RH.EnableDrawing = false;
			}
			Died = true;
			
		}

		float TimeSinceFootShuffle;

		bool duckChanged, WasDucked, HasPushed;

		VRHud worldPanel;

		//Vector3 lastPosSet;

		public void HandleHands()
		{
			if ( LH == null )
			{
				return;
			}
			LH.Transform = Input.VR.LeftHand.Transform.WithScale( 0.8f );
			RH.Transform = Input.VR.RightHand.Transform.WithScale( 0.8f );


			//LH.Position = Position + Input.VR.LeftHand.Transform.Position;

			if ( Controller == null || Controller.Pawn == null )
			{
				return;
			}

			//LH.Position += Controller.Pawn.Velocity * Time.Delta * 2.25f;
			//RH.Position += Controller.Pawn.Velocity * Time.Delta * 2.25f;

			LH.SetAnimFloat( "Thumb", Input.VR.LeftHand.GetFingerValue( FingerValue.ThumbCurl ) );
			LH.SetAnimFloat( "Index", Input.VR.LeftHand.GetFingerValue( FingerValue.IndexCurl ) );
			LH.SetAnimFloat( "Middle", Input.VR.LeftHand.GetFingerValue( FingerValue.MiddleCurl ) );
			LH.SetAnimFloat( "Ring", Input.VR.LeftHand.GetFingerValue( FingerValue.RingCurl ) );

			RH.SetAnimFloat( "Thumb", Input.VR.RightHand.GetFingerValue( FingerValue.ThumbCurl ) );
			RH.SetAnimFloat( "Index", Input.VR.RightHand.GetFingerValue( FingerValue.IndexCurl ) );
			RH.SetAnimFloat( "Middle", Input.VR.RightHand.GetFingerValue( FingerValue.MiddleCurl ) );
			RH.SetAnimFloat( "Ring", Input.VR.RightHand.GetFingerValue( FingerValue.RingCurl ) );
		}

		public void HandleTerryPuppet()
		{
			if( TerryPuppet == null )
			{
				return;
			}
			Transform LocalHead = Transform.ToLocal( Input.VR.Head );
			TerryPuppet.SetBodyGroup( 3, 1 );

			VR.Scale = 1f;

			TerryPuppet?.SetAnimBool( "b_vr", true );

			Angles puppetAng = Input.VR.Head.Rotation.Angles();
			puppetAng.roll = 0;
			puppetAng.pitch = 0;

			if ( Input.VR.Head.Rotation.Forward.z > -0.8f )
			{
				DoPuppetRotation( puppetAng.ToRotation() );
			}

			Vector3 HeadOffset = new Vector3( -10 - (((1 - (LocalHead.Position.z / 65f)) * 3f) * 10f), 0, 0 );

			TerryPuppet.Position = Position + (LocalHead.Position.WithZ( 0 ) * Rotation) + (HeadOffset * TerryPuppet.Rotation);// + LocalHead.Rotation.Forward * 100f;

			TerryPuppet.Position += Controller.Pawn.Velocity * Time.Delta * 2.25f;

			if ( LH != null )
			{

				TerryPuppet?.SetAnimVector( "left_hand_ik.position", TerryPuppet.Transform.ToLocal( LH.GetBoneTransform( 0 ) ).Position );

				TerryPuppet?.SetAnimVector( "right_hand_ik.position", TerryPuppet.Transform.ToLocal( RH.GetBoneTransform( 0 ) ).Position );

				TerryPuppet?.SetAnimRotation( "left_hand_ik.rotation", TerryPuppet.Transform.ToLocal( LH.GetBoneTransform( 0 ) ).Rotation * new Angles( 0, 0, 180 ).ToRotation() );

				TerryPuppet?.SetAnimRotation( "right_hand_ik.rotation", TerryPuppet.Transform.ToLocal( RH.GetBoneTransform( 0 ) ).Rotation );
			}

			TerryPuppet?.SetAnimFloat( "duck", (1 - (LocalHead.Position.z / 65f)) * 3f );

			
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			TimeSinceFootShuffle += Time.Delta;

			if ( TeamBased && suit.IsValid() && !Died)
			{
				suit.SetMaterialGroup( 1 );
				suit.RenderColor = (TeamColors[(PlayerNum - 1) % TeamsCount] * 0.6f).WithAlpha( 1f );
			}

			if ( IsClient && !Died)
			{
				
				foreach ( var client in Entity.All )
				{
					if ( client is not TerryPup && client is not SquidPlayer )
					{
						continue;
					}

					foreach ( Entity ent in client.Children )
					{
						if ( ent.Tags.Has( "suit" ) && (ent as ModelEntity).SceneObject != null)
						{
							//Log.Trace( "Foundsuit! " + client.Owner.NetworkIdent );
							int number = 0;
							if( client is SquidPlayer )
							{
								number = (client as SquidPlayer).PlayerNum;
							}
							else
							{
								number = (client as TerryPup).PlayerNum;
							}

							(ent as ModelEntity).SceneObject.SetValue( "pnum", number );
							break;
						}
					}
				}
				if ( TerryPuppet != null && LH != null && RH != null )
				{
					TerryPuppet.SetBone( "head", TerryPuppet.GetBoneTransform( "head" ).WithScale( 0 ) );
					TerryPuppet.SetBone( "hand_L", TerryPuppet.GetBoneTransform( "hand_L" ).WithPosition( LH.GetBoneTransform( 0 ).Position ).WithRotation( LH.GetBoneTransform( 0 ).Rotation * new Angles( 0, 0, 180 ).ToRotation() ) );
					TerryPuppet.SetBone( "hand_R", TerryPuppet.GetBoneTransform( "hand_R" ).WithPosition( RH.GetBoneTransform( 0 ).Position ).WithRotation( RH.GetBoneTransform( 0 ).Rotation ) );

					//TerryPuppet.SetBone( "hand_L", TerryPuppet.GetBoneTransform( "hand_L" ).WithPosition( LH.GetBoneTransform( 0 ).Position ) );
					//TerryPuppet.SetBone( "hand_R", TerryPuppet.GetBoneTransform( "hand_R" ).WithPosition( RH.GetBoneTransform( 0 ).Position ) );
				}
			}

			/*if ( IsServer && Input.Pressed( InputButton.Menu ) )
			{
				var ragdoll = new ModelEntity();
				ragdoll.SetModel( "models/terrybot.vmdl" );
				ragdoll.Position = Position;
				//ragdoll.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
				//ragdoll.Velocity = EyeRot.Forward * 500;
			}*/

			if(IsServer && !Input.VR.IsActive && !Died)
			{
				duckChanged = Input.Down( InputButton.Duck ) != WasDucked;
				WasDucked = Input.Down( InputButton.Duck );
				moving = Velocity.Length > 3f || duckChanged;
				headpos = GetBoneTransform( "head" ).Position;
			}

			if ( !Input.VR.IsActive && IsServer )
			{
				SetAnimBool( "b_attack", Input.Pressed( InputButton.Attack1 ) );
				if( Input.Pressed( InputButton.Attack1 ) )
				{
					Transform head = GetBoneTransform( "head" );
					Trace hittest = Trace.Ray( head.Position, head.Position + head.Rotation.Left * 60f ).Ignore( this );
					TraceResult hitResult = hittest.Run();

					//DebugOverlay.Line( head.Position, head.Position + head.Rotation.Left * 80f );

					if ( (hitResult.Entity as SquidPlayer).IsValid())
					{
						SquidPlayer playr = (hitResult.Entity as SquidPlayer);
						//Log.Trace( "Hit!" );
						playr.GroundEntity = null;
						playr.Velocity += Rotation.Forward * 150f + Vector3.Up * 300f;

						if ( Bomb.IsValid() && !playr.Bomb.IsValid() )
						{
							Bomb.SetParent( playr, "hand_L" );

							Bomb.LocalPosition = new Vector3( 3f, 0, 5f );

							Bomb.LocalRotation = new Angles( 0, 0, -90 ).ToRotation();

							playr.Bomb = Bomb;
							Bomb = null;
						}
					}
				}

				if ( Animator != null && Animator.Pawn.IsValid() )
				{
					Animator.Pawn.ActiveChild = pushAbility;
				}
			}

			if ( IsClient && Input.VR.IsActive)
			{
				if ( worldPanel == null )
				{
					worldPanel = new VRHud(PlayerNum);
					worldPanel.Transform = Input.VR.LeftHand.Transform;
				}
				worldPanel.Rotation = Input.VR.LeftHand.Transform.Rotation * new Angles( -180, -90, 45 ).ToRotation();
				worldPanel.Position = Input.VR.LeftHand.Transform.Position + worldPanel.Rotation.Forward * 3f + worldPanel.Rotation.Up * 5f - worldPanel.Rotation.Left * 2f;
				worldPanel.WorldScale = 0.25f;

				HandleTerryPuppet();
				HandleHands();
			}

			if ( Input.VR.IsActive && IsServer )
			{
				
				if ( (Input.VR.RightHand.Velocity.Length > 100f && Vector3.Dot( TerryPuppet.Rotation.Forward, Input.VR.RightHand.Velocity ) > 0) && (Input.VR.LeftHand.Velocity.Length > 100f && Vector3.Dot(TerryPuppet.Rotation.Forward, Input.VR.LeftHand.Velocity ) > 0))
				{
					Vector3 avgpos = (Input.VR.LeftHand.Transform.Position + Input.VR.RightHand.Transform.Position) / 2;
					Vector3 avgnorm = (Input.VR.LeftHand.Velocity.Normal + Input.VR.RightHand.Velocity.Normal) / 2;
					Trace hittest = Trace.Ray( avgpos, avgpos + avgnorm * 20f ).Ignore( this );
					TraceResult hitResult = hittest.Run();

					//DebugOverlay.Line( avgpos, avgpos + avgnorm * 30f );

					if ( (hitResult.Entity as SquidPlayer).IsValid() && !HasPushed )
					{
						SquidPlayer playr = (hitResult.Entity as SquidPlayer);
						playr.GroundEntity = null;
						playr.Velocity += avgnorm * 150f + Vector3.Up * 250f;

						if ( Bomb.IsValid() && !playr.Bomb.IsValid() )
						{
							Bomb.SetParent( playr, "hand_L" );

							Bomb.LocalPosition = new Vector3( 3f, 0, 5f );

							Bomb.LocalRotation = new Angles( 0, 0, -90 ).ToRotation();

							playr.Bomb = Bomb;
							Bomb = null;
						}

						HasPushed = true;
					}
				}
				else
				{
					HasPushed = false;
				}

				if ( Animator.Pawn != null )
				{
					Animator.Pawn.ActiveChild = pushAbility;
				}
			}

			if ( IsServer && Input.VR.IsActive && !Died)
			{
				moving = Velocity.Length > 3f || Input.VR.LeftHand.Velocity.Length > 10f || Input.VR.RightHand.Velocity.Length > 10f;

				if ( AnimatorRef == null )
				{
					Animator = new StandardPlayerAnimatorVR();
					AnimatorRef = Animator as StandardPlayerAnimatorVR;
				}

				if ( TerryPuppet == null )
				{
					TerryPuppet = new TerryPup();
					TerryPuppet.Owner = Client.Pawn;
					TerryPuppet.PlayerNum = PlayerNum;
					TerryPuppet.SetModel( "models/citizen/citizen.vmdl" );
					//TerryPuppet.Owner = Local.Pawn;
					//TerryPuppet.SetParent( Local.Pawn, false );
					TerryPuppet.Tags.Add( "puppet" );
					AnimatorRef.TerryPuppet = TerryPuppet;
					TerryPuppet.EnableShadowInFirstPerson = true;
					TerryPuppet.EnableHideInFirstPerson = false;

					TerryPuppet.EnableAllCollisions = true;

					TerryPuppet.Predictable = true;
					

					TerryPuppet.SetBodyGroup( 0, 0 );
					TerryPuppet.SetBodyGroup( 1, 1 );
					TerryPuppet.SetBodyGroup( 2, 1 );
					TerryPuppet.SetBodyGroup( 3, 0 );
					TerryPuppet.SetBodyGroup( 4, 0 );

					if ( suit != null )
					{
						suit.SetParent( TerryPuppet, true );
						suit.Tags.Add( "suit" );
						suit.EnableHideInFirstPerson = false;
						suit.Name = "suit";

						feet.SetParent( TerryPuppet, true );
						feet.EnableHideInFirstPerson = false;

						bottom.SetParent( TerryPuppet, true );
						bottom.EnableHideInFirstPerson = false;

						if ( hat != null )
						{
							hat.SetParent( TerryPuppet, true );
							bottom.EnableHideInFirstPerson = true;
						}

						if(!Died)
						clothes.DressEntity( TerryPuppet );
					}
				}

				if ( TerryPuppet != null )
				{
					headpos = TerryPuppet.GetBoneTransform( "head" ).Position;
				}
				//Log.Trace( TerryPuppet.Name );
				TerryPuppet.EnableShadowInFirstPerson = true;
				TerryPuppet.EnableHideInFirstPerson = false;



				if ( Bomb.IsValid() )
				{
					Bomb.SetParent( LH,"hold_L");
					
					Bomb.LocalPosition = new Vector3( -4f,-7f, 0 );

					Bomb.LocalRotation = new Angles( 45, 0, 0 ).ToRotation();

				}

				/*LH.Transform = Input.VR.LeftHand.Transform.WithScale( 0.8f );
				RH.Transform = Input.VR.RightHand.Transform.WithScale( 0.8f );

				if(Controller == null || Controller.Pawn == null)
				{
					return;
				}

				LH.Position += Controller.Pawn.Velocity * Time.Delta * 2.25f;
				RH.Position += Controller.Pawn.Velocity * Time.Delta * 2.25f;

				LH.SetAnimFloat( "Thumb", Input.VR.LeftHand.GetFingerValue( FingerValue.ThumbCurl ) );
				LH.SetAnimFloat( "Index", Input.VR.LeftHand.GetFingerValue( FingerValue.IndexCurl ) );
				LH.SetAnimFloat( "Middle", Input.VR.LeftHand.GetFingerValue( FingerValue.MiddleCurl ) );
				LH.SetAnimFloat( "Ring", Input.VR.LeftHand.GetFingerValue( FingerValue.RingCurl ) );

				RH.SetAnimFloat( "Thumb", Input.VR.RightHand.GetFingerValue( FingerValue.ThumbCurl ) );
				RH.SetAnimFloat( "Index", Input.VR.RightHand.GetFingerValue( FingerValue.IndexCurl ) );
				RH.SetAnimFloat( "Middle", Input.VR.RightHand.GetFingerValue( FingerValue.MiddleCurl ) );
				RH.SetAnimFloat( "Ring", Input.VR.RightHand.GetFingerValue( FingerValue.RingCurl ) );*/
				

				if ( AnimatorRef == null )
				{
					AnimatorRef = Animator as StandardPlayerAnimatorVR;
				}

				if ( AnimatorRef != null )
				{
					AnimatorRef.TerryPuppet = TerryPuppet;
				}

				EnableDrawing = false;
				EnableShadowInFirstPerson = false;

				HandleTerryPuppet();

				HandleHands();

				if ( LH == null )
				{
					LH = new AnimEntity();
					LH.Predictable = true;
					LH.Owner = Client.Pawn;
					LH.SetModel( "models/handleft.vmdl" );
					LH.Scale = 0.8f;
					LH.Tags.Add( "lh" );
				}

				if ( RH == null )
				{
					RH = new AnimEntity();
					RH.Predictable = true;
					RH.Owner = Client.Pawn;
					RH.SetModel( "models/handright.vmdl" );
					RH.Scale = 0.8f;
					RH.Tags.Add( "rh" );
				}
			}
		}

		/*public override void StartTouch( Entity other )
		{
			if (other is GlassPlate )
			{
				if((other as GlassPlate).breakable )
				{
					other.Delete();
					Log.Trace( "Stood on breaking glass!" );
				}
			}
			base.StartTouch( other );
		}*/

		public void DoPuppetRotation( Rotation idealRotation )
		{
			//
			// Our ideal player model rotation is the way we're facing
			//
			var allowYawDiff = 30f;// Pawn.ActiveChild == null ? 90 : 50;

			float turnSpeed = 0.5f;
			//if ( HasTag( "ducked" ) ) turnSpeed = 0.1f;

			//
			// If we're moving, rotate to our ideal rotation
			//
			TerryPuppet.Rotation = Rotation.Slerp( TerryPuppet.Rotation, idealRotation, Time.Delta * turnSpeed );

			//
			// Clamp the foot rotation to within 120 degrees of the ideal rotation
			//
			TerryPuppet.Rotation = Rotation.Slerp( TerryPuppet.Rotation, TerryPuppet.Rotation.Clamp( idealRotation, allowYawDiff, out var change ), Time.Delta * turnSpeed * 10f );

			//
			// If we did restrict, and are standing still, add a foot shuffle
			//
			if ( change > 1 ) TimeSinceFootShuffle = 0;

			TerryPuppet.SetAnimBool( "b_shuffle", TimeSinceFootShuffle < 0.1 );
		}
	}
}
