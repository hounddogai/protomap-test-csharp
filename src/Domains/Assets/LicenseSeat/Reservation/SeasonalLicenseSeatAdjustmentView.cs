namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal license seat adjustment.</summary>
public sealed record SeasonalLicenseSeatAdjustmentView(string Code, string TitleText, string Margin, string Attempts, string Source, string StatusLabel)
{
    public static SeasonalLicenseSeatAdjustmentView ToSeasonalLicenseSeatAdjustmentView(SeasonalLicenseSeatAdjustment item)
    {
        return new SeasonalLicenseSeatAdjustmentView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalLicenseSeatAdjustmentView> ToSeasonalLicenseSeatAdjustmentViews(List<SeasonalLicenseSeatAdjustment> items)
    {
        var views = new List<SeasonalLicenseSeatAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalLicenseSeatAdjustmentView(item));
        }
        return views;
    }

    public static double SumSeasonalLicenseSeatAdjustmentView(SeasonalLicenseSeatAdjustmentView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedAttempts;
    }
}
