namespace Acme.Workforce.Compensation.BonusPool.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a bonus pool snapshot.</summary>
public sealed record BonusPoolSnapshotView(string CodeText, string Name, string RateText, string Visible, string LeaveDays, string QuantityText, string ManagerId, string Badge, string TenureMonths, string StatusLabel)
{
    public static BonusPoolSnapshotView ToBonusPoolSnapshotView(BonusPoolSnapshot item)
    {
        return new BonusPoolSnapshotView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 80),
            Text.Truncate(item.Badge, 64),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolSnapshotView> ToBonusPoolSnapshotViews(List<BonusPoolSnapshot> items)
    {
        var views = new List<BonusPoolSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolSnapshotView(item));
        }
        return views;
    }

    public static double SumBonusPoolSnapshotView(BonusPoolSnapshotView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return parsedRate + (double)parsedLeaveDays + (double)parsedQuantity;
    }
}
