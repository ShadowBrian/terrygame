using Sandbox.Internal.JsonConvert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox
{
	[Library]
	public class WalkControllerFlatNoRun : WalkController
	{



		public WalkControllerFlatNoRun()
		{
			Duck = new Duck( this );
			Unstuck = new Unstuck( this );
		}

		/// <summary>
		/// This is temporary, get the hull size for the player's collision
		/// </summary>
		public override BBox GetHull()
		{
			var girth = BodyGirth * 0.5f;
			var mins = new Vector3( -girth, -girth, 0 );
			var maxs = new Vector3( +girth, +girth, BodyHeight );

			return new BBox( mins, maxs );
		}


		// Duck body height 32
		// Eye Height 64
		// Duck Eye Height 28

		public override void SetBBox( Vector3 mins, Vector3 maxs )
		{
			if ( this.mins == mins && this.maxs == maxs )
				return;

			this.mins = mins;
			this.maxs = maxs;
		}

		/// <summary>
		/// Update the size of the bbox. We should really trigger some shit if this changes.
		/// </summary>
		public override void UpdateBBox()
		{
			var girth = BodyGirth * 0.5f;

			var mins = new Vector3( -girth, -girth, 0 ) * Pawn.Scale;
			var maxs = new Vector3( +girth, +girth, BodyHeight ) * Pawn.Scale;

			Duck.UpdateBBox( ref mins, ref maxs, Pawn.Scale );

			SetBBox( mins, maxs );
		}


		public override void FrameSimulate()
		{
			base.FrameSimulate();

			EyeRot = Input.Rotation;
		}

		bool IsTouchingLadder = false;

		float BuiltUpSpeed = 0f;

		float lastframeLeft;

		public override void Simulate()
		{
			EyePosLocal = Vector3.Up * (EyeHeight * Pawn.Scale);
			UpdateBBox();

			EyePosLocal += TraceOffset;
			EyeRot = Input.Rotation;

			RestoreGroundPos();

			//Velocity += BaseVelocity * ( 1 + Time.Delta * 0.5f );
			//BaseVelocity = Vector3.Zero;

			//Rot = Rotation.LookAt( Input.Rotation.Forward.WithZ( 0 ), Vector3.Up );

			if ( Unstuck.TestAndFix() )
				return;

			// Check Stuck
			// Unstuck - or return if stuck

			// Set Ground Entity to null if  falling faster then 250

			// store water level to compare later

			// if not on ground, store fall velocity

			// player->UpdateStepSound( player->m_pSurfaceData, mv->GetAbsOrigin(), mv->m_vecVelocity )


			// RunLadderMode

			CheckLadder();
			Swimming = Pawn.WaterLevel.Fraction > 0.6f;

			//
			// Start Gravity
			//
			if ( !Swimming && !IsTouchingLadder )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
				Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;

				BaseVelocity = BaseVelocity.WithZ( 0 );
			}


			/*
             if (player->m_flWaterJumpTime)
	            {
		            WaterJump();
		            TryPlayerMove();
		            // See if we are still in water?
		            CheckWater();
		            return;
	            }
            */

			// if ( underwater ) do underwater movement

			if ( AutoJump ? Input.Down( InputButton.Jump ) : Input.Pressed( InputButton.Jump ) )
			{
				CheckJumpButton();
			}

			// Fricion is handled before we add in any base velocity. That way, if we are on a conveyor,
			//  we don't slow when standing still, relative to the conveyor.
			bool bStartOnGround = GroundEntity != null;
			//bool bDropSound = false;
			if ( bStartOnGround )
			{
				//if ( Velocity.z < FallSoundZ ) bDropSound = true;

				Velocity = Velocity.WithZ( 0 );
				//player->m_Local.m_flFallVelocity = 0.0f;

				if ( GroundEntity != null )
				{
					ApplyFriction( GroundFriction * SurfaceFriction );
				}
			}

			//
			// Work out wish velocity.. just take input, rotate it to view, clamp to -1, 1
			//
			WishVelocity = new Vector3( Input.Forward, Input.Left, 0 );
			var inSpeed = WishVelocity.Length.Clamp( 0, 1 );
			WishVelocity *= Input.Rotation;

			if ( !Swimming && !IsTouchingLadder )
			{
				WishVelocity = WishVelocity.WithZ( 0 );
			}

			WishVelocity = WishVelocity.Normal * inSpeed;
			WishVelocity *= GetWishSpeed();

			Duck.PreTick();

			bool bStayOnGround = false;
			if ( Swimming )
			{
				ApplyFriction( 1 );
				WaterMove();
			}
			else if ( IsTouchingLadder )
			{
				LadderMove();
			}
			else if ( GroundEntity != null )
			{
				bStayOnGround = true;
				WalkMove();
			}
			else
			{
				AirMove();
			}

			CategorizePosition( bStayOnGround );

			// FinishGravity
			if ( !Swimming && !IsTouchingLadder )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			}


			if ( GroundEntity != null )
			{
				Velocity = Velocity.WithZ( 0 );
			}

			// CheckFalling(); // fall damage etc

			// Land Sound
			// Swim Sounds

			SaveGroundPos();

			if ( Debug )
			{
				DebugOverlay.Box( Position + TraceOffset, mins, maxs, Color.Red );
				DebugOverlay.Box( Position, mins, maxs, Color.Blue );

				var lineOffset = 0;
				if ( Host.IsServer ) lineOffset = 10;

				DebugOverlay.ScreenText( lineOffset + 0, $"        Position: {Position}" );
				DebugOverlay.ScreenText( lineOffset + 1, $"        Velocity: {Velocity}" );
				DebugOverlay.ScreenText( lineOffset + 2, $"    BaseVelocity: {BaseVelocity}" );
				DebugOverlay.ScreenText( lineOffset + 3, $"    GroundEntity: {GroundEntity} [{GroundEntity?.Velocity}]" );
				DebugOverlay.ScreenText( lineOffset + 4, $" SurfaceFriction: {SurfaceFriction}" );
				DebugOverlay.ScreenText( lineOffset + 5, $"    WishVelocity: {WishVelocity}" );
			}

		}

		public override float GetWishSpeed()
		{
			var ws = Duck.GetWishSpeed();
			if ( ws >= 0 ) return ws;

			//if ( Input.Down( InputButton.Run ) ) return SprintSpeed;
			if ( Input.Down( InputButton.Walk ) ) return WalkSpeed;

			return DefaultSpeed;
		}

		public override void WalkMove()
		{
			var wishdir = WishVelocity.Normal;
			var wishspeed = WishVelocity.Length;

			WishVelocity = WishVelocity.WithZ( 0 );
			WishVelocity = WishVelocity.Normal * wishspeed;

			Velocity = Velocity.WithZ( 0 );
			Accelerate( wishdir, wishspeed, 0, Acceleration );
			Velocity = Velocity.WithZ( 0 );

			//   Player.SetAnimParam( "forward", Input.Forward );
			//   Player.SetAnimParam( "sideward", Input.Right );
			//   Player.SetAnimParam( "wishspeed", wishspeed );
			//    Player.SetAnimParam( "walkspeed_scale", 2.0f / 190.0f );
			//   Player.SetAnimParam( "runspeed_scale", 2.0f / 320.0f );

			//  DebugOverlay.Text( 0, Pos + Vector3.Up * 100, $"forward: {Input.Forward}\nsideward: {Input.Right}" );

			// Add in any base velocity to the current velocity.
			Velocity += BaseVelocity;

			try
			{
				if ( Velocity.Length < 1.0f )
				{
					Velocity = Vector3.Zero;
					return;
				}

				// first try just moving to the destination
				var dest = (Position + Velocity * Time.Delta).WithZ( Position.z );

				var pm = TraceBBox( Position, dest );

				if ( pm.Fraction == 1 )
				{
					Position = pm.EndPos;
					StayOnGround();
					return;
				}

				StepMove();
			}
			finally
			{

				// Now pull the base velocity back out.   Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
				Velocity -= BaseVelocity;
			}

			StayOnGround();
		}

		public override void StepMove()
		{
			MoveHelper mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn );
			mover.MaxStandableAngle = GroundAngle;

			mover.TryMoveWithStep( Time.Delta, StepSize );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		public override void Move()
		{
			MoveHelper mover = new MoveHelper( Position, Velocity );
			mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn );
			mover.MaxStandableAngle = GroundAngle;

			mover.TryMove( Time.Delta );

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		/// <summary>
		/// Add our wish direction and speed onto our velocity
		/// </summary>
		public override void Accelerate( Vector3 wishdir, float wishspeed, float speedLimit, float acceleration )
		{
			// This gets overridden because some games (CSPort) want to allow dead (observer) players
			// to be able to move around.
			// if ( !CanAccelerate() )
			//     return;

			if ( speedLimit > 0 && wishspeed > speedLimit )
				wishspeed = speedLimit;

			// See if we are changing direction a bit
			var currentspeed = Velocity.Dot( wishdir );

			// Reduce wishspeed by the amount of veer.
			var addspeed = wishspeed - currentspeed;

			// If not going to add any speed, done.
			if ( addspeed <= 0 )
				return;

			// Determine amount of acceleration.
			var accelspeed = acceleration * Time.Delta * wishspeed * SurfaceFriction;

			// Cap at addspeed
			if ( accelspeed > addspeed )
				accelspeed = addspeed;

			Velocity += wishdir * accelspeed;
		}

		/// <summary>
		/// Remove ground friction from velocity
		/// </summary>
		public override void ApplyFriction( float frictionAmount = 1.0f )
		{
			// If we are in water jump cycle, don't apply friction
			//if ( player->m_flWaterJumpTime )
			//   return;

			// Not on ground - no friction


			// Calculate speed
			var speed = Velocity.Length;
			if ( speed < 0.1f ) return;

			// Bleed off some speed, but if we have less than the bleed
			//  threshold, bleed the threshold amount.
			float control = (speed < StopSpeed) ? StopSpeed : speed;

			// Add the amount to the drop amount.
			var drop = control * Time.Delta * frictionAmount;

			// scale the velocity
			float newspeed = speed - drop;
			if ( newspeed < 0 ) newspeed = 0;

			if ( newspeed != speed )
			{
				newspeed /= speed;
				Velocity *= newspeed;
			}

			// mv->m_outWishVel -= (1.f-newspeed) * mv->m_vecVelocity;
		}

		/// <summary>
		/// Traces the current bbox and returns the result.
		/// liftFeet will move the start position up by this amount, while keeping the top of the bbox at the same
		/// position. This is good when tracing down because you won't be tracing through the ceiling above.
		/// </summary>
		public override TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
		{
			return TraceBBox( start, end, mins, maxs, liftFeet );
		}

		void RestoreGroundPos()
		{
			if ( GroundEntity == null || GroundEntity.IsWorld )
				return;

			//var Position = GroundEntity.Transform.ToWorld( GroundTransform );
			//Pos = Position.Position;
		}

		void SaveGroundPos()
		{
			if ( GroundEntity == null || GroundEntity.IsWorld )
				return;

			//GroundTransform = GroundEntity.Transform.ToLocal( new Transform( Pos, Rot ) );
		}

	}
}
