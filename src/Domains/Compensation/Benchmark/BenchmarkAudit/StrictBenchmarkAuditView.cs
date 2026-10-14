namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

using System.Globalization;

/// <summary>How screens and exports show a strict benchmark audit.</summary>
public sealed record StrictBenchmarkAuditView(string Code, string Label, string Priority, string CreatedAt, string PhoneText, string RateText, string Quantity, string StatusLabel)
{
    public static StrictBenchmarkAuditView ToStrictBenchmarkAuditView(StrictBenchmarkAudit item)
    {
        return new StrictBenchmarkAuditView(
            item.Code,
            item.Label,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictBenchmarkAuditView> ToStrictBenchmarkAuditViews(List<StrictBenchmarkAudit> items)
    {
        var views = new List<StrictBenchmarkAuditView>();
        foreach (var item in items)
        {
            views.Add(ToStrictBenchmarkAuditView(item));
        }
        return views;
    }

    public static double SumStrictBenchmarkAuditView(StrictBenchmarkAuditView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedCreatedAt + parsedRate;
    }
}
