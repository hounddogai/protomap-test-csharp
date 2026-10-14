namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy bonus pool allocation.</summary>
public sealed record LegacyBonusPoolAllocationView(string Code, string Title, string LeaveDays, string Amount, string SegmentText, string StatusLabel)
{
    public static LegacyBonusPoolAllocationView ToLegacyBonusPoolAllocationView(LegacyBonusPoolAllocation item)
    {
        return new LegacyBonusPoolAllocationView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyBonusPoolAllocationView> ToLegacyBonusPoolAllocationViews(List<LegacyBonusPoolAllocation> items)
    {
        var views = new List<LegacyBonusPoolAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyBonusPoolAllocationView(item));
        }
        return views;
    }

    public static double SumLegacyBonusPoolAllocationView(LegacyBonusPoolAllocationView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedAmount;
    }
}
