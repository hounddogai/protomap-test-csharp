namespace Acme.Workforce.Compensation.BonusPool.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool rollup.</summary>
public sealed record BonusPoolRollupView(string CodeText, string Name, string LabelText, string LeaveDays, string Priority, string OwnerId, string CeilingText, string Approved, string SalaryBandText, string StatusLabel)
{
    public static BonusPoolRollupView ToBonusPoolRollupView(BonusPoolRollup item)
    {
        return new BonusPoolRollupView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 80),
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolRollupView> ToBonusPoolRollupViews(List<BonusPoolRollup> items)
    {
        var views = new List<BonusPoolRollupView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolRollupView(item));
        }
        return views;
    }

    public static double SumBonusPoolRollupView(BonusPoolRollupView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedPriority + parsedCeiling;
    }
}
