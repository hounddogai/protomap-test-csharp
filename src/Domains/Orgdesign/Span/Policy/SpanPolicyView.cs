namespace Acme.Workforce.Orgdesign.Span.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span policy.</summary>
public sealed record SpanPolicyView(string CodeText, string TitleText, string Maximum, string LeaveDays, string UpdatedAt, string AttemptsText, string SourceText, string LocationText, string Margin, string StatusLabel)
{
    public static SpanPolicyView ToSpanPolicyView(SpanPolicy item)
    {
        return new SpanPolicyView(
            item.Code,
            item.Title,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 80),
            item.Location,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanPolicyView> ToSpanPolicyViews(List<SpanPolicy> items)
    {
        var views = new List<SpanPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToSpanPolicyView(item));
        }
        return views;
    }

    public static double SumSpanPolicyView(SpanPolicyView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedLeaveDays + (double)parsedUpdatedAt;
    }
}
