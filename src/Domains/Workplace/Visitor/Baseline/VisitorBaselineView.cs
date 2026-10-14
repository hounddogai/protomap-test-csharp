namespace Acme.Workforce.Workplace.Visitor.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor baseline.</summary>
public sealed record VisitorBaselineView(string Code, string TitleText, string Discount, string LimitText, string Version, string DurationMinutes, string StatusLabel)
{
    public static VisitorBaselineView ToVisitorBaselineView(VisitorBaseline item)
    {
        return new VisitorBaselineView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorBaselineView> ToVisitorBaselineViews(List<VisitorBaseline> items)
    {
        var views = new List<VisitorBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorBaselineView(item));
        }
        return views;
    }

    public static double SumVisitorBaselineView(VisitorBaselineView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedLimit + (double)parsedVersion;
    }
}
