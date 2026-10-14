namespace Acme.Workforce.Talent.Pool.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback pool metric.</summary>
public sealed record FallbackPoolMetricView(string Code, string Name, string CountryText, string Reviewer, string ExpiresAtText, string Score, string Revision, string BatchSize, string RegionText, string Approved, string StatusLabel)
{
    public static FallbackPoolMetricView ToFallbackPoolMetricView(FallbackPoolMetric item)
    {
        return new FallbackPoolMetricView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Country,
            Text.Truncate(item.Reviewer, 120),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPoolMetricView> ToFallbackPoolMetricViews(List<FallbackPoolMetric> items)
    {
        var views = new List<FallbackPoolMetricView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPoolMetricView(item));
        }
        return views;
    }

    public static double SumFallbackPoolMetricView(FallbackPoolMetricView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedScore + (double)parsedRevision;
    }
}
