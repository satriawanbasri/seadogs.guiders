using System;
using System.Collections.Generic;

namespace Nml.Improve.Me.Dependencies
{
	public class ApplicationViewModel
	{
		public string ReferenceNumber { get; set; }
		public string State { get; set; }
		public string FullName { get; set; }
		public DateTimeOffset AppliedOn { get; set; }
		public string SupportEmail { get; set; }
		public string Signature { get; set; }
		public LegalEntity LegalEntity { get; set; }
		public IEnumerable<Fund> PortfolioFunds { get; set; }
		public double PortfolioTotalAmount { get; set; }
	}
}