namespace Acme.Workforce.Contractors.Rate.RateProposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched rate baseline.</summary>
public sealed record BatchedRateBaselineView(string Code, string Name, string Ceiling, string DiscountText, string WindowDaysText, string Title, string Score, string TimezoneText, string Active, string NotesText, string SequenceText, string StatusLabel)
{
    public static BatchedRateBaselineView ToBatchedRateBaselineView(BatchedRateBaseline item)
    {
        return new BatchedRateBaselineView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Name, 64),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Active}",
            Text.Truncate(item.Notes, 64),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedRateBaselineView> ToBatchedRateBaselineViews(List<BatchedRateBaseline> items)
    {
        var views = new List<BatchedRateBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedRateBaselineView(item));
        }
        return views;
    }

    public static double SumBatchedRateBaselineView(BatchedRateBaselineView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedDiscount + (double)parsedWindowDays;
    }
}
