using Sandbox;

namespace terrygame
{
	public partial class TerryPup : AnimEntity
	{
		[Net]
		public int PlayerNum { get; set; }
	}
}
