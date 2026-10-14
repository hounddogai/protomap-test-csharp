namespace Acme.Workforce.Engagement.ActionPlan.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual action plan entry.</summary>
public sealed record ManualActionPlanEntryView(string Code, string Label, string UpdatedAt, string DurationMinutes, string ExternalRef, string Phone, string WeightText, string Country, string TitleText, string WindowDays, string StatusLabel)
{
    public static ManualActionPlanEntryView ToManualActionPlanEntryView(ManualActionPlanEntry item)
    {
        return new ManualActionPlanEntryView(
            item.Code,
            item.Label,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Phone,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 120),
            item.Title,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualActionPlanEntryView> ToManualActionPlanEntryViews(List<ManualActionPlanEntry> items)
    {
        var views = new List<ManualActionPlanEntryView>();
        foreach (var item in items)
        {
            views.Add(ToManualActionPlanEntryView(item));
        }
        return views;
    }

    public static double SumManualActionPlanEntryView(ManualActionPlanEntryView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedDurationMinutes + parsedWeight;
    }
}
