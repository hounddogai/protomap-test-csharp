namespace Acme.Workforce.Compensation.Adjustment.Notice;

using System.Globalization;

/// <summary>How screens and exports show a adjustment slot.</summary>
public sealed record AdjustmentSlotView(string CodeText, string Title, string WindowDaysText, string Rating, string GraceDays, string CreatedAt, string Discount, string StatusLabel)
{
    public static AdjustmentSlotView ToAdjustmentSlotView(AdjustmentSlot item)
    {
        return new AdjustmentSlotView(
            item.Code,
            item.Title,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentSlotView> ToAdjustmentSlotViews(List<AdjustmentSlot> items)
    {
        var views = new List<AdjustmentSlotView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentSlotView(item));
        }
        return views;
    }

    public static double SumAdjustmentSlotView(AdjustmentSlotView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedRating + (double)parsedGraceDays;
    }
}
