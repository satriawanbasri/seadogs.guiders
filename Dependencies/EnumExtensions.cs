namespace Nml.Improve.Me.Dependencies
{
	public static class EnumExtensions
	{
		public static string ToDescription(this ApplicationState state)
		{
			return state.ToString();
		}
	}
}