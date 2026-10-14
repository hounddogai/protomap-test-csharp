namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat estimate.</summary>
public sealed record LicenseSeatEstimateView(string Code, string Title, string RegionText, string Checksum, string Label, string JobCode, string DurationMinutes, string FloorValueText, string EndsAt, string StatusLabel)
{
    public static LicenseSeatEstimateView ToLicenseSeatEstimateView(LicenseSeatEstimate item)
    {
        return new LicenseSeatEstimateView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.Region, 64),
            Text.Truncate(item.Checksum, 64),
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.JobCode, 40),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatEstimateView> ToLicenseSeatEstimateViews(List<LicenseSeatEstimate> items)
    {
        var views = new List<LicenseSeatEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatEstimateView(item));
        }
        return views;
    }

    public static double SumLicenseSeatEstimateView(LicenseSeatEstimateView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedFloorValue + (double)parsedEndsAt;
    }
}
