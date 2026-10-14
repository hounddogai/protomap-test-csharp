namespace Acme.Workforce.Contractors.Engagement.Digest;

using System.Globalization;

/// <summary>How screens and exports show a rolling engagement digest.</summary>
public sealed record RollingEngagementDigestView(string Code, string Label, string Retries, string BadgeText, string Fte, string CostCenter, string Active, string StatusLabel)
{
    public static RollingEngagementDigestView ToRollingEngagementDigestView(RollingEngagementDigest item)
    {
        return new RollingEngagementDigestView(
            item.Code,
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingEngagementDigestView> ToRollingEngagementDigestViews(List<RollingEngagementDigest> items)
    {
        var views = new List<RollingEngagementDigestView>();
        foreach (var item in items)
        {
            views.Add(ToRollingEngagementDigestView(item));
        }
        return views;
    }

    public static double SumRollingEngagementDigestView(RollingEngagementDigestView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedFte;
    }
}
