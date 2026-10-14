namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient benchmark audit.</summary>
public sealed record LenientBenchmarkAuditView(string Code, string TitleText, string EmployeeIdText, string Minimum, string ManagerId, string Sequence, string Cost, string Phone, string LimitText, string Locale, string StatusLabel)
{
    public static LenientBenchmarkAuditView ToLenientBenchmarkAuditView(LenientBenchmarkAudit item)
    {
        return new LenientBenchmarkAuditView(
            Text.Truncate(item.Code, 64),
            item.Title,
            Text.Truncate(item.EmployeeId, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientBenchmarkAuditView> ToLenientBenchmarkAuditViews(List<LenientBenchmarkAudit> items)
    {
        var views = new List<LenientBenchmarkAuditView>();
        foreach (var item in items)
        {
            views.Add(ToLenientBenchmarkAuditView(item));
        }
        return views;
    }

    public static double SumLenientBenchmarkAuditView(LenientBenchmarkAuditView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedSequence + parsedCost;
    }
}
