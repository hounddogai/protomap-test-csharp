namespace Acme.Workforce.Compensation.BonusPool.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default bonus pool checkpoint.</summary>
public sealed record DefaultBonusPoolCheckpointView(string Code, string NameText, string Quantity, string Rating, string Timezone, string TenureMonthsText, string BudgetText, string StatusLabel)
{
    public static DefaultBonusPoolCheckpointView ToDefaultBonusPoolCheckpointView(DefaultBonusPoolCheckpoint item)
    {
        return new DefaultBonusPoolCheckpointView(
            item.Code,
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 64),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultBonusPoolCheckpointView> ToDefaultBonusPoolCheckpointViews(List<DefaultBonusPoolCheckpoint> items)
    {
        var views = new List<DefaultBonusPoolCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultBonusPoolCheckpointView(item));
        }
        return views;
    }

    public static double SumDefaultBonusPoolCheckpointView(DefaultBonusPoolCheckpointView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedRating + (double)parsedTenureMonths;
    }
}
