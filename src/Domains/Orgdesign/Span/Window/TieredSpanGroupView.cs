namespace Acme.Workforce.Orgdesign.Span.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered span group.</summary>
public sealed record TieredSpanGroupView(string CodeText, string TitleText, string Label, string Visible, string Category, string Amount, string Checksum, string Unit, string Source, string StatusLabel)
{
    public static TieredSpanGroupView ToTieredSpanGroupView(TieredSpanGroup item)
    {
        return new TieredSpanGroupView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 80),
            item.Label,
            $"{item.Visible}",
            item.Category,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            item.Unit,
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredSpanGroupView> ToTieredSpanGroupViews(List<TieredSpanGroup> items)
    {
        var views = new List<TieredSpanGroupView>();
        foreach (var item in items)
        {
            views.Add(ToTieredSpanGroupView(item));
        }
        return views;
    }

    public static double SumTieredSpanGroupView(TieredSpanGroupView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
