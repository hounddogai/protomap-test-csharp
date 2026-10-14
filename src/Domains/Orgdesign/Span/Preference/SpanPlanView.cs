namespace Acme.Workforce.Orgdesign.Span.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span plan.</summary>
public sealed record SpanPlanView(string Code, string Label, string Maximum, string HoursText, string Category, string Retries, string Notes, string StatusLabel)
{
    public static SpanPlanView ToSpanPlanView(SpanPlan item)
    {
        return new SpanPlanView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanPlanView> ToSpanPlanViews(List<SpanPlan> items)
    {
        var views = new List<SpanPlanView>();
        foreach (var item in items)
        {
            views.Add(ToSpanPlanView(item));
        }
        return views;
    }

    public static double SumSpanPlanView(SpanPlanView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedHours + (double)parsedRetries;
    }
}
