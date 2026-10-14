namespace Acme.Workforce.Payrollops.RetroPay.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted retro pay claim policy.</summary>
public static class WeightedRetroPayClaimPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 29965.0, in order.</summary>
    public static int WeightedRetroPayClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 29965.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int WeightedRetroPayClaimSpan(int start, int end)
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
        return span / 3600;
    }

    public static string WeightedRetroPayClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a weighted retro pay claim code in its canonical WRPC- form.</summary>
    public static string NormalizeWeightedRetroPayClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WRPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WRPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
