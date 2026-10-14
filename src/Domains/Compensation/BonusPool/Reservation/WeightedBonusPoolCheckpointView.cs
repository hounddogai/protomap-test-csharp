namespace Acme.Workforce.Compensation.BonusPool.Reservation;

using System.Globalization;

/// <summary>How screens and exports show a weighted bonus pool checkpoint.</summary>
public sealed record WeightedBonusPoolCheckpointView(string Code, string Label, string AmountText, string Ceiling, string Category, string WindowDays, string ExternalRefText, string Checksum, string StatusLabel)
{
    public static WeightedBonusPoolCheckpointView ToWeightedBonusPoolCheckpointView(WeightedBonusPoolCheckpoint item)
    {
        return new WeightedBonusPoolCheckpointView(
            item.Code,
            item.Label,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedBonusPoolCheckpointView> ToWeightedBonusPoolCheckpointViews(List<WeightedBonusPoolCheckpoint> items)
    {
        var views = new List<WeightedBonusPoolCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedBonusPoolCheckpointView(item));
        }
        return views;
    }

    public static double SumWeightedBonusPoolCheckpointView(WeightedBonusPoolCheckpointView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return parsedAmount + parsedCeiling + (double)parsedWindowDays;
    }
}
