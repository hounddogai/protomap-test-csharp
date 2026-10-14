namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal adjustment reservation.</summary>
public sealed record SeasonalAdjustmentReservationView(string CodeText, string LabelText, string SourceText, string Amount, string VersionText, string CategoryText, string Description, string FloorValueText, string BatchSize, string StatusLabel)
{
    public static SeasonalAdjustmentReservationView ToSeasonalAdjustmentReservationView(SeasonalAdjustmentReservation item)
    {
        return new SeasonalAdjustmentReservationView(
            item.Code,
            item.Label,
            Text.Truncate(item.Source, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            item.Description,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalAdjustmentReservationView> ToSeasonalAdjustmentReservationViews(List<SeasonalAdjustmentReservation> items)
    {
        var views = new List<SeasonalAdjustmentReservationView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalAdjustmentReservationView(item));
        }
        return views;
    }

    public static double SumSeasonalAdjustmentReservationView(SeasonalAdjustmentReservationView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedVersion + parsedFloorValue;
    }
}
