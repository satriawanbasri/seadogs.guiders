namespace Nml.Improve.Me.Dependencies
{
	public interface IViewGenerator
	{
		string GenerateFromPath(string url, object viewModel);
	}
}