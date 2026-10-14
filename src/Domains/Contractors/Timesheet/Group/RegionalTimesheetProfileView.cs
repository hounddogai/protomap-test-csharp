namespace Acme.Workforce.Contractors.Timesheet.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional timesheet profile.</summary>
public sealed record RegionalTimesheetProfileView(string Code, string Name, string FloorValue, string PhoneText, string Limit, string Timezone, string GradeText, string Margin, string BatchSize, string StatusLabel)
{
    public static RegionalTimesheetProfileView ToRegionalTimesheetProfileView(RegionalTimesheetProfile item)
    {
        return new RegionalTimesheetProfileView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 80),
            item.Grade,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalTimesheetProfileView> ToRegionalTimesheetProfileViews(List<RegionalTimesheetProfile> items)
    {
        var views = new List<RegionalTimesheetProfileView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalTimesheetProfileView(item));
        }
        return views;
    }

    public static double SumRegionalTimesheetProfileView(RegionalTimesheetProfileView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedLimit + parsedMargin;
    }
}
