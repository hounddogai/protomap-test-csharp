namespace Acme.Workforce.Orgdesign.Span.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span profile.</summary>
public sealed record SpanProfileView(string Code, string Label, string CostCenterText, string Source, string Amount, string SalaryBandText, string StatusLabel)
{
    public static SpanProfileView ToSpanProfileView(SpanProfile item)
    {
        return new SpanProfileView(
            item.Code,
            item.Label,
            Text.Truncate(item.CostCenter, 120),
            Text.Truncate(item.Source, 40),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanProfileView> ToSpanProfileViews(List<SpanProfile> items)
    {
        var views = new List<SpanProfileView>();
        foreach (var item in items)
        {
            views.Add(ToSpanProfileView(item));
        }
        return views;
    }

    public static double SumSpanProfileView(SpanProfileView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
