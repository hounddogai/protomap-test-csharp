namespace Acme.Workforce.Talent.Pool.Rule;

using System.Globalization;

/// <summary>How screens and exports show a scheduled pool rule.</summary>
public sealed record ScheduledPoolRuleView(string Code, string TitleText, string Minimum, string Visible, string DurationMinutesText, string CapacityText, string LimitText, string WindowDays, string MaximumText, string StatusLabel)
{
    public static ScheduledPoolRuleView ToScheduledPoolRuleView(ScheduledPoolRule item)
    {
        return new ScheduledPoolRuleView(
            item.Code,
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPoolRuleView> ToScheduledPoolRuleViews(List<ScheduledPoolRule> items)
    {
        var views = new List<ScheduledPoolRuleView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPoolRuleView(item));
        }
        return views;
    }

    public static double SumScheduledPoolRuleView(ScheduledPoolRuleView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedDurationMinutes + (double)parsedCapacity;
    }
}
