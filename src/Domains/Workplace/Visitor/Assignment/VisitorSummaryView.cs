namespace Acme.Workforce.Workplace.Visitor.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor summary.</summary>
public sealed record VisitorSummaryView(string CodeText, string Title, string CeilingText, string EndsAt, string Source, string FteText, string MaximumText, string Quantity, string StatusLabel)
{
    public static VisitorSummaryView ToVisitorSummaryView(VisitorSummary item)
    {
        return new VisitorSummaryView(
            item.Code,
            item.Title,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 64),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorSummaryView> ToVisitorSummaryViews(List<VisitorSummary> items)
    {
        var views = new List<VisitorSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorSummaryView(item));
        }
        return views;
    }

    public static double SumVisitorSummaryView(VisitorSummaryView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return parsedCeiling + (double)parsedEndsAt + parsedFte;
    }
}
