namespace Acme.Workforce.Orgdesign.Span.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped span slot.</summary>
public sealed record CappedSpanSlotView(string Code, string Label, string SalaryBand, string Phone, string Name, string UnitText, string Title, string SummaryText, string Score, string StatusLabel)
{
    public static CappedSpanSlotView ToCappedSpanSlotView(CappedSpanSlot item)
    {
        return new CappedSpanSlotView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.SalaryBand,
            Text.Truncate(item.Phone, 64),
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.Unit, 40),
            Text.Truncate(item.Title, 40),
            Text.Truncate(item.Summary, 80),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedSpanSlotView> ToCappedSpanSlotViews(List<CappedSpanSlot> items)
    {
        var views = new List<CappedSpanSlotView>();
        foreach (var item in items)
        {
            views.Add(ToCappedSpanSlotView(item));
        }
        return views;
    }

    public static double SumCappedSpanSlotView(CappedSpanSlotView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
