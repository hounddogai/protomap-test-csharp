namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using System.Globalization;

/// <summary>How screens and exports show a rolling bonus pool slot.</summary>
public sealed record RollingBonusPoolSlotView(string CodeText, string Title, string Rate, string Retries, string Name, string LeaveDays, string Source, string SalaryBand, string RatingText, string Priority, string StatusLabel)
{
    public static RollingBonusPoolSlotView ToRollingBonusPoolSlotView(RollingBonusPoolSlot item)
    {
        return new RollingBonusPoolSlotView(
            item.Code,
            item.Title,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.SalaryBand,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingBonusPoolSlotView> ToRollingBonusPoolSlotViews(List<RollingBonusPoolSlot> items)
    {
        var views = new List<RollingBonusPoolSlotView>();
        foreach (var item in items)
        {
            views.Add(ToRollingBonusPoolSlotView(item));
        }
        return views;
    }

    public static double SumRollingBonusPoolSlotView(RollingBonusPoolSlotView view)
    {
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedRetries + (double)parsedLeaveDays;
    }
}
