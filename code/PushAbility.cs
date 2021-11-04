using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace terrygame
{
	partial class PushAbility : BaseCarriable
	{
		public override void SimulateAnimator( PawnAnimator anim )
		{
			anim.SetParam( "holdtype", Input.Down( InputButton.Attack1 )?5:0 );
			anim.SetParam( "aimat_weight", 1.0f );
		}

		public Entity Push()
		{
			Entity hit;
			Transform head = (this.Parent as SquidPlayer).GetBoneTransform( "head" );
			Trace hittest = Trace.Ray( head.Position, head.Position + head.Rotation.Forward * 80f ).Ignore(Parent);
			TraceResult hitResult = hittest.Run();

			hit = hitResult.Entity;

			return hit;
		}
	}
}
