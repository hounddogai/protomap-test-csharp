namespace Acme.Workforce.Contractors.Rate.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rate estimate policy.</summary>
public static class RateEstimatePolicy
{
    /// <summary>Puts a rate estimate code in its canonical RE- form.</summary>
    public static string NormalizeRateEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string RateEstimateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 8780.0, in order.</summary>
    public static int RateEstimateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8780.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int RateEstimateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }
}
