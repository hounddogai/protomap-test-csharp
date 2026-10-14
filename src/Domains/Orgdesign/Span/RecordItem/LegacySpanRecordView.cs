namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy span record.</summary>
public sealed record LegacySpanRecordView(string CodeText, string Title, string QuantityText, string Region, string CostText, string ManagerId, string EndsAt, string StatusLabel)
{
    public static LegacySpanRecordView ToLegacySpanRecordView(LegacySpanRecord item)
    {
        return new LegacySpanRecordView(
            item.Code,
            item.Title,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 120),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacySpanRecordView> ToLegacySpanRecordViews(List<LegacySpanRecord> items)
    {
        var views = new List<LegacySpanRecordView>();
        foreach (var item in items)
        {
            views.Add(ToLegacySpanRecordView(item));
        }
        return views;
    }

    public static double SumLegacySpanRecordView(LegacySpanRecordView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + parsedCost + (double)parsedEndsAt;
    }
}
