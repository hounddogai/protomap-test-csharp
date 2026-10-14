namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using System.Globalization;

/// <summary>How screens and exports show a regional timesheet milestone.</summary>
public sealed record RegionalTimesheetMilestoneView(string Code, string Name, string Capacity, string ScoreText, string Cost, string Ceiling, string CostCenterText, string StatusLabel)
{
    public static RegionalTimesheetMilestoneView ToRegionalTimesheetMilestoneView(RegionalTimesheetMilestone item)
    {
        return new RegionalTimesheetMilestoneView(
            item.Code,
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalTimesheetMilestoneView> ToRegionalTimesheetMilestoneViews(List<RegionalTimesheetMilestone> items)
    {
        var views = new List<RegionalTimesheetMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalTimesheetMilestoneView(item));
        }
        return views;
    }

    public static double SumRegionalTimesheetMilestoneView(RegionalTimesheetMilestoneView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedScore + parsedCost;
    }
}
