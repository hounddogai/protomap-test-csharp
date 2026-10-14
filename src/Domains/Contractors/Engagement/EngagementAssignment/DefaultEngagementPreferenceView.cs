namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default engagement preference.</summary>
public sealed record DefaultEngagementPreferenceView(string Code, string Name, string CostCenterText, string LabelText, string PinnedText, string Limit, string StatusLabel)
{
    public static DefaultEngagementPreferenceView ToDefaultEngagementPreferenceView(DefaultEngagementPreference item)
    {
        return new DefaultEngagementPreferenceView(
            Text.Truncate(item.Code, 64),
            item.Name,
            Text.Truncate(item.CostCenter, 80),
            item.Label,
            $"{item.Pinned}",
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultEngagementPreferenceView> ToDefaultEngagementPreferenceViews(List<DefaultEngagementPreference> items)
    {
        var views = new List<DefaultEngagementPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultEngagementPreferenceView(item));
        }
        return views;
    }

    public static double SumDefaultEngagementPreferenceView(DefaultEngagementPreferenceView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedLimit;
    }
}
