namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient garnishment estimate policy.</summary>
public static class LenientGarnishmentEstimatePolicy
{
    /// <summary>Puts a lenient garnishment estimate code in its canonical LGE- form.</summary>
    public static string NormalizeLenientGarnishmentEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LGE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LGE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LenientGarnishmentEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 8624.0, in order.</summary>
    public static int LenientGarnishmentEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8624.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a lenient garnishment estimate score falls in.</summary>
    public static string ClassifyLenientGarnishmentEstimate(double score)
    {
        if (score < 21.1)
        {
            return "low";
        }
        if (score < 71.7)
        {
            return "high";
        }
        return "medium";
    }
}
