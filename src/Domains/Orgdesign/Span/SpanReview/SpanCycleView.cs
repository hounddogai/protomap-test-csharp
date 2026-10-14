namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span cycle.</summary>
public sealed record SpanCycleView(string Code, string Title, string TimezoneText, string Department, string EmployeeId, string SalaryBandText, string Hours, string RetriesText, string BatchSize, string Unit, string StatusLabel)
{
    public static SpanCycleView ToSpanCycleView(SpanCycle item)
    {
        return new SpanCycleView(
            item.Code,
            item.Title,
            item.Timezone,
            item.Department,
            item.EmployeeId,
            item.SalaryBand,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanCycleView> ToSpanCycleViews(List<SpanCycle> items)
    {
        var views = new List<SpanCycleView>();
        foreach (var item in items)
        {
            views.Add(ToSpanCycleView(item));
        }
        return views;
    }

    public static double SumSpanCycleView(SpanCycleView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedRetries + (double)parsedBatchSize;
    }
}
