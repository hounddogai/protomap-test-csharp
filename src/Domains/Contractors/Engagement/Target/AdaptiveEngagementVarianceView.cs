namespace Acme.Workforce.Contractors.Engagement.Target;

using System.Globalization;

/// <summary>How screens and exports show a adaptive engagement variance.</summary>
public sealed record AdaptiveEngagementVarianceView(string Code, string Label, string Badge, string Segment, string Source, string LeaveDays, string StatusLabel)
{
    public static AdaptiveEngagementVarianceView ToAdaptiveEngagementVarianceView(AdaptiveEngagementVariance item)
    {
        return new AdaptiveEngagementVarianceView(
            item.Code,
            item.Label,
            item.Badge,
            item.Segment,
            item.Source,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveEngagementVarianceView> ToAdaptiveEngagementVarianceViews(List<AdaptiveEngagementVariance> items)
    {
        var views = new List<AdaptiveEngagementVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveEngagementVarianceView(item));
        }
        return views;
    }

    public static double SumAdaptiveEngagementVarianceView(AdaptiveEngagementVarianceView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays;
    }
}
