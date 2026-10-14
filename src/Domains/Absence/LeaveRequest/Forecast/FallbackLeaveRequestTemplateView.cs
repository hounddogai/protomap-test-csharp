namespace Acme.Workforce.Absence.LeaveRequest.Forecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback leave request template.</summary>
public sealed record FallbackLeaveRequestTemplateView(string Code, string Name, string DurationMinutes, string FloorValue, string Grade, string Notes, string TimezoneText, string ChecksumText, string Region, string StatusLabel)
{
    public static FallbackLeaveRequestTemplateView ToFallbackLeaveRequestTemplateView(FallbackLeaveRequestTemplate item)
    {
        return new FallbackLeaveRequestTemplateView(
            item.Code,
            item.Name,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            Text.Truncate(item.Notes, 80),
            item.Timezone,
            item.Checksum,
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackLeaveRequestTemplateView> ToFallbackLeaveRequestTemplateViews(List<FallbackLeaveRequestTemplate> items)
    {
        var views = new List<FallbackLeaveRequestTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackLeaveRequestTemplateView(item));
        }
        return views;
    }

    public static double SumFallbackLeaveRequestTemplateView(FallbackLeaveRequestTemplateView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedFloorValue;
    }
}
