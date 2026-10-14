namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a license seat metric.</summary>
public sealed record LicenseSeatMetricView(string Code, string LabelText, string FloorValueText, string JobCode, string GraceDays, string ContactEmail, string Cost, string StartsAt, string StatusLabel)
{
    public static LicenseSeatMetricView ToLicenseSeatMetricView(LicenseSeatMetric item)
    {
        return new LicenseSeatMetricView(
            item.Code,
            item.Label,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 80),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LicenseSeatMetricView> ToLicenseSeatMetricViews(List<LicenseSeatMetric> items)
    {
        var views = new List<LicenseSeatMetricView>();
        foreach (var item in items)
        {
            views.Add(ToLicenseSeatMetricView(item));
        }
        return views;
    }

    public static double SumLicenseSeatMetricView(LicenseSeatMetricView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedGraceDays + parsedCost;
    }
}
