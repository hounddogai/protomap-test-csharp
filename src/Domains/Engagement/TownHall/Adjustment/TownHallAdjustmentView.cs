namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a town hall adjustment.</summary>
public sealed record TownHallAdjustmentView(string Code, string Label, string WindowDaysText, string ExternalRef, string FloorValueText, string UpdatedAtText, string Attempts, string SalaryBandText, string Hours, string ContactEmailText, string Retries, string StatusLabel)
{
    public static TownHallAdjustmentView ToTownHallAdjustmentView(TownHallAdjustment item)
    {
        return new TownHallAdjustmentView(
            item.Code,
            item.Label,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallAdjustmentView> ToTownHallAdjustmentViews(List<TownHallAdjustment> items)
    {
        var views = new List<TownHallAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallAdjustmentView(item));
        }
        return views;
    }

    public static double SumTownHallAdjustmentView(TownHallAdjustmentView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedFloorValue + (double)parsedUpdatedAt;
    }
}
