using System.Linq;

namespace Nml.Improve.Me.Dependencies
{
	public interface IDataContext
	{
		IQueryable<Application> Applications { get; set; }
	}
}