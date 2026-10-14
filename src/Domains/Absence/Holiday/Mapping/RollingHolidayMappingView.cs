namespace Acme.Workforce.Absence.Holiday.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling holiday mapping.</summary>
public sealed record RollingHolidayMappingView(string Code, string TitleText, string EmployeeIdText, string Summary, string Threshold, string PriorityText, string StatusLabel)
{
    public static RollingHolidayMappingView ToRollingHolidayMappingView(RollingHolidayMapping item)
    {
        return new RollingHolidayMappingView(
            item.Code,
            item.Title,
            item.EmployeeId,
            Text.Truncate(item.Summary, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingHolidayMappingView> ToRollingHolidayMappingViews(List<RollingHolidayMapping> items)
    {
        var views = new List<RollingHolidayMappingView>();
        foreach (var item in items)
        {
            views.Add(ToRollingHolidayMappingView(item));
        }
        return views;
    }

    public static double SumRollingHolidayMappingView(RollingHolidayMappingView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedPriority;
    }
}
