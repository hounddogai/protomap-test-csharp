namespace Acme.Workforce.Orgdesign.Span.Ledger;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span tier.</summary>
public sealed record SpanTierView(string Code, string NameText, string Hours, string Reviewer, string ExternalRef, string Department, string Rate, string Team, string Currency, string Quantity, string Approved, string StatusLabel)
{
    public static SpanTierView ToSpanTierView(SpanTier item)
    {
        return new SpanTierView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.ExternalRef,
            item.Department,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.Currency,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanTierView> ToSpanTierViews(List<SpanTier> items)
    {
        var views = new List<SpanTierView>();
        foreach (var item in items)
        {
            views.Add(ToSpanTierView(item));
        }
        return views;
    }

    public static double SumSpanTierView(SpanTierView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedHours + parsedRate + (double)parsedQuantity;
    }
}
