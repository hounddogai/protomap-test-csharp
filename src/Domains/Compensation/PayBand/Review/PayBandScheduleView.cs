namespace Acme.Workforce.Compensation.PayBand.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay band schedule.</summary>
public sealed record PayBandScheduleView(string CodeText, string Label, string Threshold, string Version, string DescriptionText, string Department, string FloorValueText, string Locale, string StatusLabel)
{
    public static PayBandScheduleView ToPayBandScheduleView(PayBandSchedule item)
    {
        return new PayBandScheduleView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Description,
            Text.Truncate(item.Department, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayBandScheduleView> ToPayBandScheduleViews(List<PayBandSchedule> items)
    {
        var views = new List<PayBandScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToPayBandScheduleView(item));
        }
        return views;
    }

    public static double SumPayBandScheduleView(PayBandScheduleView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedVersion + parsedFloorValue;
    }
}
