namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool ledger.</summary>
public sealed record BonusPoolLedgerView(string CodeText, string TitleText, string Skill, string Weight, string Description, string Quantity, string Hours, string SegmentText, string Locale, string StatusLabel)
{
    public static BonusPoolLedgerView ToBonusPoolLedgerView(BonusPoolLedger item)
    {
        return new BonusPoolLedgerView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Skill,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolLedgerView> ToBonusPoolLedgerViews(List<BonusPoolLedger> items)
    {
        var views = new List<BonusPoolLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolLedgerView(item));
        }
        return views;
    }

    public static double SumBonusPoolLedgerView(BonusPoolLedgerView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedQuantity + parsedHours;
    }
}
