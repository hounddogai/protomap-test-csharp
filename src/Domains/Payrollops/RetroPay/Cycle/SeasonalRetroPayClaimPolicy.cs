namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal retro pay claim policy.</summary>
public static class SeasonalRetroPayClaimPolicy
{
    /// <summary>Names the band a seasonal retro pay claim score falls in.</summary>
    public static string ClassifySeasonalRetroPayClaim(double score)
    {
        if (score < 44.9)
        {
            return "ok";
        }
        if (score < 54.2)
        {
            return "minimal";
        }
        if (score < 54.4)
        {
            return "high";
        }
        if (score < 98.3)
        {
            return "critical";
        }
        return "medium";
    }

    public static string SeasonalRetroPayClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 92859.0, in order.</summary>
    public static int SeasonalRetroPayClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92859.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a seasonal retro pay claim code in its canonical SRPC- form.</summary>
    public static string NormalizeSeasonalRetroPayClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SRPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SRPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
