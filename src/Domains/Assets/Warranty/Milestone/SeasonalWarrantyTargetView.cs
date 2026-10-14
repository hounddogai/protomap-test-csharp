namespace Acme.Workforce.Assets.Warranty.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal warranty target.</summary>
public sealed record SeasonalWarrantyTargetView(string Code, string TitleText, string FloorValue, string AttemptsText, string Country, string ExternalRef, string JobCode, string Notes, string LocationText, string StatusLabel)
{
    public static SeasonalWarrantyTargetView ToSeasonalWarrantyTargetView(SeasonalWarrantyTarget item)
    {
        return new SeasonalWarrantyTargetView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.ExternalRef,
            item.JobCode,
            Text.Truncate(item.Notes, 40),
            Text.Truncate(item.Location, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalWarrantyTargetView> ToSeasonalWarrantyTargetViews(List<SeasonalWarrantyTarget> items)
    {
        var views = new List<SeasonalWarrantyTargetView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalWarrantyTargetView(item));
        }
        return views;
    }

    public static double SumSeasonalWarrantyTargetView(SeasonalWarrantyTargetView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedAttempts;
    }
}
