namespace Acme.Workforce.Contractors.Offboarding.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding segment.</summary>
public sealed record OffboardingSegmentView(string Code, string Name, string Ratio, string Summary, string CeilingText, string ExpiresAtText, string Retries, string StatusLabel)
{
    public static OffboardingSegmentView ToOffboardingSegmentView(OffboardingSegment item)
    {
        return new OffboardingSegmentView(
            item.Code,
            item.Name,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 64),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingSegmentView> ToOffboardingSegmentViews(List<OffboardingSegment> items)
    {
        var views = new List<OffboardingSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingSegmentView(item));
        }
        return views;
    }

    public static double SumOffboardingSegmentView(OffboardingSegmentView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return parsedRatio + parsedCeiling + (double)parsedExpiresAt;
    }
}
