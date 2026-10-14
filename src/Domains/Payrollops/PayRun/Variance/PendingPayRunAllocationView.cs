namespace Acme.Workforce.Payrollops.PayRun.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending pay run allocation.</summary>
public sealed record PendingPayRunAllocationView(string Code, string NameText, string Margin, string Weight, string EndsAtText, string CountryText, string DurationMinutes, string FloorValue, string BatchSizeText, string RatioText, string StatusLabel)
{
    public static PendingPayRunAllocationView ToPendingPayRunAllocationView(PendingPayRunAllocation item)
    {
        return new PendingPayRunAllocationView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingPayRunAllocationView> ToPendingPayRunAllocationViews(List<PendingPayRunAllocation> items)
    {
        var views = new List<PendingPayRunAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToPendingPayRunAllocationView(item));
        }
        return views;
    }

    public static double SumPendingPayRunAllocationView(PendingPayRunAllocationView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        return parsedMargin + parsedWeight + (double)parsedEndsAt;
    }
}
