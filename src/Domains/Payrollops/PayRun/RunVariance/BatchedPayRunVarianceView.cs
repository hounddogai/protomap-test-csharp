namespace Acme.Workforce.Payrollops.PayRun.RunVariance;

using System.Globalization;

/// <summary>How screens and exports show a batched pay run variance.</summary>
public sealed record BatchedPayRunVarianceView(string Code, string Title, string DiscountText, string ExpiresAtText, string TeamText, string Summary, string Skill, string FloorValue, string Minimum, string SalaryBand, string CostCenter, string StatusLabel)
{
    public static BatchedPayRunVarianceView ToBatchedPayRunVarianceView(BatchedPayRunVariance item)
    {
        return new BatchedPayRunVarianceView(
            item.Code,
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.Summary,
            item.Skill,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedPayRunVarianceView> ToBatchedPayRunVarianceViews(List<BatchedPayRunVariance> items)
    {
        var views = new List<BatchedPayRunVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedPayRunVarianceView(item));
        }
        return views;
    }

    public static double SumBatchedPayRunVarianceView(BatchedPayRunVarianceView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedExpiresAt + parsedFloorValue;
    }
}
