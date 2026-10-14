namespace Acme.Workforce.Talent.Pool.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered pool metric.</summary>
public sealed record TieredPoolMetricView(string CodeText, string Label, string ExternalRef, string ContactEmail, string Title, string PinnedText, string Checksum, string Revision, string StatusLabel)
{
    public static TieredPoolMetricView ToTieredPoolMetricView(TieredPoolMetric item)
    {
        return new TieredPoolMetricView(
            item.Code,
            item.Label,
            item.ExternalRef,
            Text.Truncate(item.ContactEmail, 120),
            item.Title,
            $"{item.Pinned}",
            item.Checksum,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPoolMetricView> ToTieredPoolMetricViews(List<TieredPoolMetric> items)
    {
        var views = new List<TieredPoolMetricView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPoolMetricView(item));
        }
        return views;
    }

    public static double SumTieredPoolMetricView(TieredPoolMetricView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
