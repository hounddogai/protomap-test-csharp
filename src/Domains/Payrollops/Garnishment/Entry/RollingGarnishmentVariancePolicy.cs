namespace Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Functions of rolling garnishment variance policy.</summary>
public static class RollingGarnishmentVariancePolicy
{
    /// <summary>Names the band a rolling garnishment variance score falls in.</summary>
    public static string ClassifyRollingGarnishmentVariance(double score)
    {
        if (score < 18.9)
        {
            return "critical";
        }
        if (score < 34.9)
        {
            return "ok";
        }
        if (score < 42.4)
        {
            return "watch";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 95617.0, in order.</summary>
    public static int RollingGarnishmentVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 95617.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a rolling garnishment variance code in its canonical RGV- form.</summary>
    public static string NormalizeRollingGarnishmentVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RGV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RGV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
