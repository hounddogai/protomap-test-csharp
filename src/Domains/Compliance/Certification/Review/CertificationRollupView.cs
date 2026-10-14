namespace Acme.Workforce.Compliance.Certification.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification rollup.</summary>
public sealed record CertificationRollupView(string Code, string Label, string CostCenter, string Location, string EndsAt, string Region, string StartsAt, string SegmentText, string Channel, string Pinned, string StatusLabel)
{
    public static CertificationRollupView ToCertificationRollupView(CertificationRollup item)
    {
        return new CertificationRollupView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 120),
            Text.Truncate(item.CostCenter, 40),
            item.Location,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 64),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Channel,
            $"{item.Pinned}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationRollupView> ToCertificationRollupViews(List<CertificationRollup> items)
    {
        var views = new List<CertificationRollupView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationRollupView(item));
        }
        return views;
    }

    public static double SumCertificationRollupView(CertificationRollupView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedStartsAt;
    }
}
