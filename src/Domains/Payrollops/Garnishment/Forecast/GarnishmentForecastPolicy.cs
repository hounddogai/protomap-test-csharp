namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of garnishment forecast policy.</summary>
public static class GarnishmentForecastPolicy
{
    /// <summary>Puts a garnishment forecast code in its canonical GF- form.</summary>
    public static string NormalizeGarnishmentForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a garnishment forecast score falls in.</summary>
    public static string ClassifyGarnishmentForecast(double score)
    {
        if (score < 33.0)
        {
            return "medium";
        }
        if (score < 38.2)
        {
            return "elevated";
        }
        if (score < 67.7)
        {
            return "critical";
        }
        if (score < 87.6)
        {
            return "ok";
        }
        return "minimal";
    }

    public static List<string> MergeGarnishmentForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 73346.0, in order.</summary>
    public static int GarnishmentForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 73346.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
