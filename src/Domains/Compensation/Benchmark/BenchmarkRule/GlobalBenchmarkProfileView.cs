namespace Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global benchmark profile.</summary>
public sealed record GlobalBenchmarkProfileView(string CodeText, string Label, string OwnerIdText, string EmployeeId, string ContactEmail, string Fte, string Hours, string DurationMinutes, string StatusLabel)
{
    public static GlobalBenchmarkProfileView ToGlobalBenchmarkProfileView(GlobalBenchmarkProfile item)
    {
        return new GlobalBenchmarkProfileView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.OwnerId,
            item.EmployeeId,
            item.ContactEmail,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalBenchmarkProfileView> ToGlobalBenchmarkProfileViews(List<GlobalBenchmarkProfile> items)
    {
        var views = new List<GlobalBenchmarkProfileView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalBenchmarkProfileView(item));
        }
        return views;
    }

    public static double SumGlobalBenchmarkProfileView(GlobalBenchmarkProfileView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedFte + parsedHours + (double)parsedDurationMinutes;
    }
}
