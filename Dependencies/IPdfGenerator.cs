namespace Nml.Improve.Me.Dependencies
{
	public interface IPdfGenerator
	{
		PdfDocument GenerateFromHtml(string view, PdfOptions pdfOptions);
	}
}