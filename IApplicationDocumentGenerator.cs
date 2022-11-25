using System;

namespace Nml.Improve.Me
{
	public interface IApplicationDocumentGenerator
	{
		byte[] Generate(Guid applicationId, string baseUri);
	}
}