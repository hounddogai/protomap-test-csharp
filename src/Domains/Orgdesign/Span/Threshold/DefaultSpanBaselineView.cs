namespace Acme.Workforce.Orgdesign.Span.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default span baseline.</summary>
public sealed record DefaultSpanBaselineView(string CodeText, string Name, string LabelText, string ArchivedText, string SummaryText, string JobCodeText, string Phone, string Amount, string StatusLabel)
{
    public static DefaultSpanBaselineView ToDefaultSpanBaselineView(DefaultSpanBaseline item)
    {
        return new DefaultSpanBaselineView(
            Text.Truncate(item.Code, 64),
            item.Name,
            Text.Truncate(item.Label, 40),
            $"{item.Archived}",
            item.Summary,
            item.JobCode,
            item.Phone,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultSpanBaselineView> ToDefaultSpanBaselineViews(List<DefaultSpanBaseline> items)
    {
        var views = new List<DefaultSpanBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultSpanBaselineView(item));
        }
        return views;
    }

    public static double SumDefaultSpanBaselineView(DefaultSpanBaselineView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
