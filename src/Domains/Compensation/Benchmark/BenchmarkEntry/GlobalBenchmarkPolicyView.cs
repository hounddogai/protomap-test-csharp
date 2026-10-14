namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global benchmark policy.</summary>
public sealed record GlobalBenchmarkPolicyView(string Code, string Name, string FteText, string CategoryText, string Maximum, string CostCenter, string Limit, string Threshold, string ContactEmail, string StatusLabel)
{
    public static GlobalBenchmarkPolicyView ToGlobalBenchmarkPolicyView(GlobalBenchmarkPolicy item)
    {
        return new GlobalBenchmarkPolicyView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalBenchmarkPolicyView> ToGlobalBenchmarkPolicyViews(List<GlobalBenchmarkPolicy> items)
    {
        var views = new List<GlobalBenchmarkPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalBenchmarkPolicyView(item));
        }
        return views;
    }

    public static double SumGlobalBenchmarkPolicyView(GlobalBenchmarkPolicyView view)
    {
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedFte + parsedMaximum + (double)parsedLimit;
    }
}
