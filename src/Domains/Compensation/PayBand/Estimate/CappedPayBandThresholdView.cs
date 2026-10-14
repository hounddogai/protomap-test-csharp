namespace Acme.Workforce.Compensation.PayBand.Estimate;

using System.Globalization;

/// <summary>How screens and exports show a capped pay band threshold.</summary>
public sealed record CappedPayBandThresholdView(string Code, string LabelText, string DurationMinutesText, string ApprovedText, string LeaveDays, string CostCenter, string Discount, string StatusLabel)
{
    public static CappedPayBandThresholdView ToCappedPayBandThresholdView(CappedPayBandThreshold item)
    {
        return new CappedPayBandThresholdView(
            item.Code,
            item.Label,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedPayBandThresholdView> ToCappedPayBandThresholdViews(List<CappedPayBandThreshold> items)
    {
        var views = new List<CappedPayBandThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToCappedPayBandThresholdView(item));
        }
        return views;
    }

    public static double SumCappedPayBandThresholdView(CappedPayBandThresholdView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedLeaveDays + parsedDiscount;
    }
}
