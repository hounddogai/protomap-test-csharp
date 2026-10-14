namespace Acme.Workforce.Talent.CareerPath.PathRule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered career path forecast policy.</summary>
public static class TieredCareerPathForecastPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 58954.0, in order.</summary>
    public static int TieredCareerPathForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58954.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a tiered career path forecast code in its canonical TCPF- form.</summary>
    public static string NormalizeTieredCareerPathForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TCPF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TCPF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TieredCareerPathForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    public static List<string> MergeTieredCareerPathForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
