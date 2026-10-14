namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using System.Globalization;

/// <summary>How screens and exports show a seasonal leave request rule.</summary>
public sealed record SeasonalLeaveRequestRuleView(string CodeText, string Label, string Locale, string Timezone, string Revision, string StatusLabel)
{
    public static SeasonalLeaveRequestRuleView ToSeasonalLeaveRequestRuleView(SeasonalLeaveRequestRule item)
    {
        return new SeasonalLeaveRequestRuleView(
            item.Code,
            item.Label,
            item.Locale,
            item.Timezone,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalLeaveRequestRuleView> ToSeasonalLeaveRequestRuleViews(List<SeasonalLeaveRequestRule> items)
    {
        var views = new List<SeasonalLeaveRequestRuleView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalLeaveRequestRuleView(item));
        }
        return views;
    }

    public static double SumSeasonalLeaveRequestRuleView(SeasonalLeaveRequestRuleView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
