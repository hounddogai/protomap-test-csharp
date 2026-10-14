namespace Acme.Workforce.Compliance.Certification.CertificationAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered certification summary.</summary>
public sealed record TieredCertificationSummaryView(string CodeText, string Title, string Maximum, string Capacity, string Notes, string GraceDays, string StatusLabel)
{
    public static TieredCertificationSummaryView ToTieredCertificationSummaryView(TieredCertificationSummary item)
    {
        return new TieredCertificationSummaryView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 120),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredCertificationSummaryView> ToTieredCertificationSummaryViews(List<TieredCertificationSummary> items)
    {
        var views = new List<TieredCertificationSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToTieredCertificationSummaryView(item));
        }
        return views;
    }

    public static double SumTieredCertificationSummaryView(TieredCertificationSummaryView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedCapacity + (double)parsedGraceDays;
    }
}
