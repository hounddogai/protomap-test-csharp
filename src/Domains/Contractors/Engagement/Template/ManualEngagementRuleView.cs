namespace Acme.Workforce.Contractors.Engagement.Template;

using System.Globalization;

/// <summary>How screens and exports show a manual engagement rule.</summary>
public sealed record ManualEngagementRuleView(string Code, string Label, string StartsAtText, string Amount, string VersionText, string StatusLabel)
{
    public static ManualEngagementRuleView ToManualEngagementRuleView(ManualEngagementRule item)
    {
        return new ManualEngagementRuleView(
            item.Code,
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualEngagementRuleView> ToManualEngagementRuleViews(List<ManualEngagementRule> items)
    {
        var views = new List<ManualEngagementRuleView>();
        foreach (var item in items)
        {
            views.Add(ToManualEngagementRuleView(item));
        }
        return views;
    }

    public static double SumManualEngagementRuleView(ManualEngagementRuleView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedAmount + (double)parsedVersion;
    }
}
