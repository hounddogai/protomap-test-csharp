namespace Acme.Workforce.Absence.Holiday.Metric;

using System.Globalization;

/// <summary>How screens and exports show a default holiday milestone.</summary>
public sealed record DefaultHolidayMilestoneView(string Code, string Label, string LeaveDays, string Weight, string Budget, string Pinned, string ExpiresAtText, string Ceiling, string GraceDays, string StatusLabel)
{
    public static DefaultHolidayMilestoneView ToDefaultHolidayMilestoneView(DefaultHolidayMilestone item)
    {
        return new DefaultHolidayMilestoneView(
            item.Code,
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultHolidayMilestoneView> ToDefaultHolidayMilestoneViews(List<DefaultHolidayMilestone> items)
    {
        var views = new List<DefaultHolidayMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultHolidayMilestoneView(item));
        }
        return views;
    }

    public static double SumDefaultHolidayMilestoneView(DefaultHolidayMilestoneView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedWeight + parsedBudget;
    }
}
