namespace Sandbox
{
	[Library]
	public class NoclipControllerVR : BasePlayerController
	{
		public Vector2 LeftJoy;

		public override void Simulate()
		{
			var vel = new Vector3( Input.VR.LeftHand.Joystick.Value.y, -Input.VR.LeftHand.Joystick.Value.x, 0 );
			vel *= Input.VR.Head.Rotation; // Multiply by Input.VR.LeftHand.Transform.Rotation to make it hand-directed

			if ( Input.VR.RightHand.ButtonA.IsPressed )
			{
				vel += Vector3.Up * 1;
			}

			vel = vel.Normal * 2000;

			if ( Input.VR.LeftHand.Trigger.Value>0.5f )
				vel *= 5.0f;

			if ( Input.VR.RightHand.Trigger.Value > 0.5f )
				vel *= 0.2f;

			Velocity += vel * Time.Delta;

			if ( Velocity.LengthSquared > 0.01f )
			{
				Position += Velocity * Time.Delta;
			}

			Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );


			EyeRot = Input.VR.Head.Rotation;
			WishVelocity = Velocity;
			GroundEntity = null;
			BaseVelocity = Vector3.Zero;

			SetTag( "noclip" );
		}

	}
}
