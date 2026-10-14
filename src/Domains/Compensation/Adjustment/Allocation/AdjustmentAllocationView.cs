namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment allocation.</summary>
public sealed record AdjustmentAllocationView(string Code, string Name, string RatioText, string TimezoneText, string Currency, string Sequence, string UpdatedAt, string ExpiresAt, string LocationText, string TenureMonthsText, string EndsAtText, string StatusLabel)
{
    public static AdjustmentAllocationView ToAdjustmentAllocationView(AdjustmentAllocation item)
    {
        return new AdjustmentAllocationView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Currency,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentAllocationView> ToAdjustmentAllocationViews(List<AdjustmentAllocation> items)
    {
        var views = new List<AdjustmentAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentAllocationView(item));
        }
        return views;
    }

    public static double SumAdjustmentAllocationView(AdjustmentAllocationView view)
    {
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedRatio + (double)parsedSequence + (double)parsedUpdatedAt;
    }
}
