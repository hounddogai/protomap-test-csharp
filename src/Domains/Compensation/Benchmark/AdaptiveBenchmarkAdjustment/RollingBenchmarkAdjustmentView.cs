namespace Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling benchmark adjustment.</summary>
public sealed record RollingBenchmarkAdjustmentView(string Code, string Name, string GradeText, string Title, string WindowDays, string GraceDaysText, string Label, string ExternalRef, string Location, string Retries, string Margin, string StatusLabel)
{
    public static RollingBenchmarkAdjustmentView ToRollingBenchmarkAdjustmentView(RollingBenchmarkAdjustment item)
    {
        return new RollingBenchmarkAdjustmentView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Grade,
            Text.Truncate(item.Title, 80),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.ExternalRef,
            Text.Truncate(item.Location, 64),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingBenchmarkAdjustmentView> ToRollingBenchmarkAdjustmentViews(List<RollingBenchmarkAdjustment> items)
    {
        var views = new List<RollingBenchmarkAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToRollingBenchmarkAdjustmentView(item));
        }
        return views;
    }

    public static double SumRollingBenchmarkAdjustmentView(RollingBenchmarkAdjustmentView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedGraceDays + (double)parsedRetries;
    }
}
