namespace Acme.Workforce.Compliance.Certification.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification summary.</summary>
public sealed record CertificationSummaryView(string Code, string Label, string Maximum, string Cost, string Margin, string Source, string CostCenter, string Approved, string Badge, string StatusLabel)
{
    public static CertificationSummaryView ToCertificationSummaryView(CertificationSummary item)
    {
        return new CertificationSummaryView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 80),
            Text.Truncate(item.CostCenter, 40),
            $"{item.Approved}",
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationSummaryView> ToCertificationSummaryViews(List<CertificationSummary> items)
    {
        var views = new List<CertificationSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationSummaryView(item));
        }
        return views;
    }

    public static double SumCertificationSummaryView(CertificationSummaryView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedCost + parsedMargin;
    }
}
