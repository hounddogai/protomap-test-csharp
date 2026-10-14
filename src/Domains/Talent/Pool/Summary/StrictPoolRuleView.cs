namespace Acme.Workforce.Talent.Pool.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict pool rule.</summary>
public sealed record StrictPoolRuleView(string CodeText, string Title, string Ceiling, string Minimum, string RegionText, string DurationMinutesText, string ManagerId, string GraceDays, string StatusLabel)
{
    public static StrictPoolRuleView ToStrictPoolRuleView(StrictPoolRule item)
    {
        return new StrictPoolRuleView(
            item.Code,
            item.Title,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 80),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPoolRuleView> ToStrictPoolRuleViews(List<StrictPoolRule> items)
    {
        var views = new List<StrictPoolRuleView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPoolRuleView(item));
        }
        return views;
    }

    public static double SumStrictPoolRuleView(StrictPoolRuleView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedMinimum + (double)parsedDurationMinutes;
    }
}
