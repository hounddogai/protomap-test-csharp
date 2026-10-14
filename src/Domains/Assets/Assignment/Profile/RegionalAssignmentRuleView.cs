namespace Acme.Workforce.Assets.Assignment.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional assignment rule.</summary>
public sealed record RegionalAssignmentRuleView(string CodeText, string LabelText, string StartsAt, string Margin, string ExpiresAtText, string Summary, string Weight, string LeaveDaysText, string Visible, string Rate, string StatusLabel)
{
    public static RegionalAssignmentRuleView ToRegionalAssignmentRuleView(RegionalAssignmentRule item)
    {
        return new RegionalAssignmentRuleView(
            item.Code,
            item.Label,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 64),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalAssignmentRuleView> ToRegionalAssignmentRuleViews(List<RegionalAssignmentRule> items)
    {
        var views = new List<RegionalAssignmentRuleView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalAssignmentRuleView(item));
        }
        return views;
    }

    public static double SumRegionalAssignmentRuleView(RegionalAssignmentRuleView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + parsedMargin + (double)parsedExpiresAt;
    }
}
