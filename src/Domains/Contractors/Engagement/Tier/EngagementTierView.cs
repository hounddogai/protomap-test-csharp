namespace Acme.Workforce.Contractors.Engagement.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a engagement tier.</summary>
public sealed record EngagementTierView(string Code, string Label, string Sequence, string Timezone, string RevisionText, string Summary, string StatusLabel)
{
    public static EngagementTierView ToEngagementTierView(EngagementTier item)
    {
        return new EngagementTierView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EngagementTierView> ToEngagementTierViews(List<EngagementTier> items)
    {
        var views = new List<EngagementTierView>();
        foreach (var item in items)
        {
            views.Add(ToEngagementTierView(item));
        }
        return views;
    }

    public static double SumEngagementTierView(EngagementTierView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedRevision;
    }
}
