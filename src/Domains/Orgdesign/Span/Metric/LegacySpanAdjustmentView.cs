namespace Acme.Workforce.Orgdesign.Span.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy span adjustment.</summary>
public sealed record LegacySpanAdjustmentView(string Code, string Name, string EndsAtText, string Weight, string Sequence, string Discount, string CurrencyText, string CostCenter, string OwnerId, string StatusLabel)
{
    public static LegacySpanAdjustmentView ToLegacySpanAdjustmentView(LegacySpanAdjustment item)
    {
        return new LegacySpanAdjustmentView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.CostCenter,
            Text.Truncate(item.OwnerId, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacySpanAdjustmentView> ToLegacySpanAdjustmentViews(List<LegacySpanAdjustment> items)
    {
        var views = new List<LegacySpanAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToLegacySpanAdjustmentView(item));
        }
        return views;
    }

    public static double SumLegacySpanAdjustmentView(LegacySpanAdjustmentView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedWeight + (double)parsedSequence;
    }
}
