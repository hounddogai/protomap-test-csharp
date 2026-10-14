namespace Acme.Workforce.Workplace.Desk.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive desk batch.</summary>
public sealed record AdaptiveDeskBatchView(string Code, string Title, string SegmentText, string Discount, string SalaryBand, string Amount, string RetriesText, string StatusLabel)
{
    public static AdaptiveDeskBatchView ToAdaptiveDeskBatchView(AdaptiveDeskBatch item)
    {
        return new AdaptiveDeskBatchView(
            item.Code,
            item.Title,
            Text.Truncate(item.Segment, 80),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveDeskBatchView> ToAdaptiveDeskBatchViews(List<AdaptiveDeskBatch> items)
    {
        var views = new List<AdaptiveDeskBatchView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveDeskBatchView(item));
        }
        return views;
    }

    public static double SumAdaptiveDeskBatchView(AdaptiveDeskBatchView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedAmount + (double)parsedRetries;
    }
}
