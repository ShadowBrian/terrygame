using Sandbox;


namespace terrygame
{
	partial class SquidPlayer : Player
	{
		ModelEntity suit, feet, bottom, hat;

		Clothing.Container clothes = new();

		public Vector3 headpos;

		[Net, Predicted]
		AnimEntity LH { get; set; }

		[Net, Predicted]
		AnimEntity RH { get; set; }

		[Net, Predicted]
		public TerryPup TerryPuppet { get; set; }

		public TerryGame.GameTypes gametype;

		public bool moving;

		WalkControllerVR ControllerRef;
		StandardPlayerAnimatorVR AnimatorRef;

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
					Controller = new WalkController();
				}

				Animator = new StandardPlayerAnimator();

				Camera = new ThirdPersonCamera();

				EnableAllCollisions = true;
				EnableDrawing = true;
				EnableHideInFirstPerson = true;
				EnableShadowInFirstPerson = true;

			}

			if ( suit == null )
			{

				suit = new ModelEntity();
				suit.SetModel( "models/clothes/tracksuit_top.vmdl" );
				suit.Tags.Add( "suit" );
				suit.SetParent( this, true );
				suit.EnableShadowInFirstPerson = true;
				suit.EnableHideInFirstPerson = true;

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

				

				clothes.LoadFromClient(Client);

				for ( int i = 0; i < clothes.Clothing.Count; i++ )
				{
					if ( clothes.Clothing[i].Category == Clothing.ClothingCategory.Bottoms || clothes.Clothing[i].Category == Clothing.ClothingCategory.Footwear || clothes.Clothing[i].Category == Clothing.ClothingCategory.Tops )
					{
						clothes.Clothing[i] = new Clothing();
					}
				}


				clothes.DressEntity( this );
				

				/*if ( Rand.Int( 0, 3 ) != 1 )
				{
					var model = Rand.FromArray( new[]
					{
			"models/citizen_clothes/hair/hair_looseblonde/hair_looseblonde.vmdl",
			"models/citizen_clothes/hair/hair_malestyle02.vmdl",
			"models/citizen_clothes/hair/hair_looseblonde/hair_looseblonde.vmdl",
			"models/citizen_clothes/hair/hair_malestyle02.vmdl",
			"models/citizen_clothes/hair/hair_femalebun.black.vmdl",
			"models/citizen_clothes/hair/hair_femalebun.blonde.vmdl",
			"models/citizen_clothes/hair/hair_femalebun.brown.vmdl",
			"models/citizen_clothes/hair/hair_femalebun.red.vmdl"
				} );

					hat = new ModelEntity();
					hat.SetModel( model );
					hat.SetParent( this, true );
					hat.EnableShadowInFirstPerson = true;
					hat.EnableHideInFirstPerson = true;
				}*/
			}

			/*if ( Rand.Int( 0, 3 ) != 1 && (hat == null || !hat.Name.Contains("female")))
			{
				var model = Rand.FromArray( new[]
				{
				"models/citizen_clothes/beards/beard_trucker_black.vmdl",
				"models/citizen_clothes/beards/beard_trucker_blonde.vmdl",
				"models/citizen_clothes/beards/beard_trucker_brown.vmdl",
				"models/citizen_clothes/beards/beard_trucker_ginger.vmdl",
				"models/citizen_clothes/beards/beard_trucker_white.vmdl",
				"models/citizen_clothes/beards/moustache.vmdl"
			} );

				hat = new ModelEntity();
				hat.SetModel( model );
				hat.SetParent( this, true );
				hat.EnableShadowInFirstPerson = true;
				hat.EnableHideInFirstPerson = true;
			}*/

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

			//ResetRotation();
			//if ( AnimatorRef != null )
			//AnimatorRef.PlayerRot = Rotation.Identity.Angles();

			//Controller = null;


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

		bool duckChanged, WasDucked;

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			TimeSinceFootShuffle += Time.Delta;

			if ( IsClient && !Died)
			{
				foreach ( var client in Entity.All )
				{
					if ( client is not TerryPup && client is not SquidPlayer && !client.Tags.Has( "lh" ) && !client.Tags.Has( "rh" ) )
					{
						continue;
					}

					if ( client.Tags.Has( "puppet" ) )
					{
						TerryPuppet = (TerryPup)client;
					}

					if ( client.Tags.Has( "lh" ) )
					{
						LH = (AnimEntity)client;
					}
					if ( client.Tags.Has( "rh" ) )
					{
						RH = (AnimEntity)client;
					}

					foreach ( Entity ent in client.Children )
					{
						if ( ent.Tags.Has( "suit" ) )
						{
							//Log.Trace( "Foundsuit! " + ent.Name );
							suit = (ModelEntity)ent;
							break;
						}


					}
					if ( suit != null && client.Owner != null )
					{
						suit.SceneObject.SetValue( "pnum", client.Owner.NetworkIdent );
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

			if ( IsServer && Input.VR.IsActive && !Died)
			{
				moving = Velocity.Length > 3f || Input.VR.LeftHand.Velocity.Length > 10f || Input.VR.RightHand.Velocity.Length > 10f;

				Transform LocalHead = Transform.ToLocal( Input.VR.Head );
				if ( AnimatorRef == null )
				{
					Animator = new StandardPlayerAnimatorVR();
					AnimatorRef = Animator as StandardPlayerAnimatorVR;
				}

				if ( TerryPuppet == null )
				{
					TerryPuppet = new TerryPup();
					TerryPuppet.SetModel( "models/citizen/citizen.vmdl" );
					TerryPuppet.Owner = Local.Pawn;
					//TerryPuppet.SetParent( Local.Pawn, false );
					TerryPuppet.Tags.Add( "puppet" );
					AnimatorRef.TerryPuppet = TerryPuppet;
					TerryPuppet.EnableShadowInFirstPerson = true;
					TerryPuppet.EnableHideInFirstPerson = false;

					TerryPuppet.EnableAllCollisions = true;
					

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

				if ( LH == null )
				{
					LH = new AnimEntity();
					LH.SetModel( "models/handleft.vmdl" );
					LH.Scale = 0.8f;
					LH.Tags.Add( "lh" );
				}

				if ( RH == null )
				{
					RH = new AnimEntity();
					RH.SetModel( "models/handright.vmdl" );
					RH.Scale = 0.8f;
					RH.Tags.Add( "rh" );
				}

				LH.Transform = Input.VR.LeftHand.Transform.WithScale( 0.8f );
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
				RH.SetAnimFloat( "Ring", Input.VR.RightHand.GetFingerValue( FingerValue.RingCurl ) );

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

				TerryPuppet?.SetAnimVector( "left_hand_ik.position", TerryPuppet.Transform.ToLocal( LH.GetBoneTransform( 0 ) ).Position );

				TerryPuppet?.SetAnimVector( "right_hand_ik.position", TerryPuppet.Transform.ToLocal( RH.GetBoneTransform( 0 ) ).Position );

				TerryPuppet?.SetAnimRotation( "left_hand_ik.rotation", TerryPuppet.Transform.ToLocal( LH.GetBoneTransform( 0 ) ).Rotation * new Angles( 0, 0, 180 ).ToRotation() );

				TerryPuppet?.SetAnimRotation( "right_hand_ik.rotation", TerryPuppet.Transform.ToLocal( RH.GetBoneTransform( 0 ) ).Rotation );

				TerryPuppet?.SetAnimFloat( "duck", (1 - (LocalHead.Position.z / 65f)) * 3f );
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
