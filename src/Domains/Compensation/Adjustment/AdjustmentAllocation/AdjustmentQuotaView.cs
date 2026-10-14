namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using System.Globalization;

/// <summary>How screens and exports show a adjustment quota.</summary>
public sealed record AdjustmentQuotaView(string Code, string Name, string Threshold, string TenureMonths, string FloorValue, string Discount, string ContactEmailText, string ManagerId, string StatusLabel)
{
    public static AdjustmentQuotaView ToAdjustmentQuotaView(AdjustmentQuota item)
    {
        return new AdjustmentQuotaView(
            item.Code,
            item.Name,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentQuotaView> ToAdjustmentQuotaViews(List<AdjustmentQuota> items)
    {
        var views = new List<AdjustmentQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentQuotaView(item));
        }
        return views;
    }

    public static double SumAdjustmentQuotaView(AdjustmentQuotaView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedTenureMonths + parsedFloorValue;
    }
}
