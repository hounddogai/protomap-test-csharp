namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal license seat window.</summary>
public sealed record SeasonalLicenseSeatWindowView(string Code, string TitleText, string Grade, string RatioText, string FloorValue, string StatusLabel)
{
    public static SeasonalLicenseSeatWindowView ToSeasonalLicenseSeatWindowView(SeasonalLicenseSeatWindow item)
    {
        return new SeasonalLicenseSeatWindowView(
            item.Code,
            item.Title,
            Text.Truncate(item.Grade, 80),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalLicenseSeatWindowView> ToSeasonalLicenseSeatWindowViews(List<SeasonalLicenseSeatWindow> items)
    {
        var views = new List<SeasonalLicenseSeatWindowView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalLicenseSeatWindowView(item));
        }
        return views;
    }

    public static double SumSeasonalLicenseSeatWindowView(SeasonalLicenseSeatWindowView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedRatio + parsedFloorValue;
    }
}
