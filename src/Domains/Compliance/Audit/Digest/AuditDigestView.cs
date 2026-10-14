namespace Acme.Workforce.Compliance.Audit.Digest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit digest.</summary>
public sealed record AuditDigestView(string Code, string Name, string SalaryBand, string Retries, string Archived, string LocationText, string RateText, string DurationMinutes, string GraceDays, string StatusLabel)
{
    public static AuditDigestView ToAuditDigestView(AuditDigest item)
    {
        return new AuditDigestView(
            item.Code,
            item.Name,
            item.SalaryBand,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            Text.Truncate(item.Location, 120),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditDigestView> ToAuditDigestViews(List<AuditDigest> items)
    {
        var views = new List<AuditDigestView>();
        foreach (var item in items)
        {
            views.Add(ToAuditDigestView(item));
        }
        return views;
    }

    public static double SumAuditDigestView(AuditDigestView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedRate + (double)parsedDurationMinutes;
    }
}
