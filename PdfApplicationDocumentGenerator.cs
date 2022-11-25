using Nml.Improve.Me.Dependencies;
using System;
using System.Linq;

namespace Nml.Improve.Me
{
	public class PdfApplicationDocumentGenerator : IApplicationDocumentGenerator
	{
		private readonly IDataContext _dataContext;
		private IPathProvider _templatePathProvider;
		public IViewGenerator View_Generator;
		internal readonly IConfiguration _configuration;
		private readonly IPdfGenerator _pdfGenerator;
		private readonly ILogger<PdfApplicationDocumentGenerator> _logger;

		public PdfApplicationDocumentGenerator(
			IDataContext dataContext,
			IPathProvider templatePathProvider,
			IViewGenerator viewGenerator,
			IConfiguration configuration,
			IPdfGenerator pdfGenerator,
			ILogger<PdfApplicationDocumentGenerator> logger)
		{
			if (dataContext != null)
				throw new ArgumentNullException(nameof(dataContext));

			_dataContext = dataContext;
			_templatePathProvider = templatePathProvider ?? throw new ArgumentNullException("templatePathProvider");
			View_Generator = viewGenerator;
			_configuration = configuration;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_pdfGenerator = pdfGenerator;
		}

		public byte[] Generate(Guid applicationId, string baseUri)
		{
			var application = _dataContext.Applications.Single(app => app.Id == applicationId);

			if (application != null)
			{
				if (baseUri.EndsWith("/"))
					baseUri = baseUri.Substring(baseUri.Length - 1);

				string view, path;

				switch (application.State)
				{
					case ApplicationState.Pending:
						path = _templatePathProvider.Get("PendingApplication");
						var pendingApplicationViewModel = new PendingApplicationViewModel
						{
							ReferenceNumber = application.ReferenceNumber,
							State = application.State.ToDescription(),
							FullName = application.Person.FirstName + " " + application.Person.Surname,
							AppliedOn = application.Date,
							SupportEmail = _configuration.SupportEmail,
							Signature = _configuration.Signature
						};
						view = View_Generator.GenerateFromPath(baseUri + path, pendingApplicationViewModel);
						break;
					case ApplicationState.Activated:
						path = _templatePathProvider.Get("ActivatedApplication");
						var activatedApplicationViewModel = new ActivatedApplicationViewModel
						{
							ReferenceNumber = application.ReferenceNumber,
							State = application.State.ToDescription(),
							FullName = application.Person.FirstName + " " + application.Person.Surname,
							LegalEntity = application.IsLegalEntity ? application.LegalEntity : null,
							PortfolioFunds = application.Products.SelectMany(p => p.Funds),
							PortfolioTotalAmount = application.Products.SelectMany(p => p.Funds).Select(f => (f.Amount - f.Fees) * _configuration.TaxRate).Sum(),
							AppliedOn = application.Date,
							SupportEmail = _configuration.SupportEmail,
							Signature = _configuration.Signature
						};
						view = View_Generator.GenerateFromPath(baseUri + path, activatedApplicationViewModel);
						break;
					case ApplicationState.InReview:
						path = _templatePathProvider.Get("InReviewApplication");
						var inReviewMessage = "Your application has been placed in review" +
											application.CurrentReview.Reason switch
											{
												{ } reason when reason.Contains("address") =>
													" pending outstanding address verification for FICA purposes.",
												{ } reason when reason.Contains("bank") =>
													" pending outstanding bank account verification.",
												_ =>
													" because of suspicious account behaviour. Please contact support ASAP."
											};
						var inReviewApplicationViewModel = new InReviewApplicationViewModel()
						{
							ReferenceNumber = application.ReferenceNumber,
							State = application.State.ToDescription(),
							FullName = application.Person.FirstName + " " + application.Person.Surname,
							LegalEntity = application.IsLegalEntity ? application.LegalEntity : null,
							PortfolioFunds = application.Products.SelectMany(p => p.Funds),
							PortfolioTotalAmount = application.Products.SelectMany(p => p.Funds).Select(f => (f.Amount - f.Fees) * _configuration.TaxRate).Sum(),
							InReviewMessage = inReviewMessage,
							InReviewInformation = application.CurrentReview,
							AppliedOn = application.Date,
							SupportEmail = _configuration.SupportEmail,
							Signature = _configuration.Signature
						};
						view = View_Generator.GenerateFromPath(baseUri + path, inReviewApplicationViewModel);
						break;
					default:
						_logger.LogWarning(
							$"The application is in state '{application.State}' and no valid document can be generated for it.");
						return null;
				}

				var pdfOptions = new PdfOptions
				{
					PageNumbers = PageNumbers.Numeric,
					HeaderOptions = new HeaderOptions
					{
						HeaderRepeat = HeaderRepeat.FirstPageOnly,
						HeaderHtml = PdfConstants.Header
					}
				};
				var pdf = _pdfGenerator.GenerateFromHtml(view, pdfOptions);
				return pdf.ToBytes();
			}
			else
			{
				_logger.LogWarning(
					$"No application found for id '{applicationId}'");
				return null;
			}
		}
	}
}
