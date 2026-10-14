namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary retro pay preference policy.</summary>
public static class PrimaryRetroPayPreferencePolicy
{
    /// <summary>Puts a primary retro pay preference code in its canonical PRPP- form.</summary>
    public static string NormalizePrimaryRetroPayPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PrimaryRetroPayPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 15844.0, in order.</summary>
    public static int PrimaryRetroPayPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15844.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a primary retro pay preference score falls in.</summary>
    public static string ClassifyPrimaryRetroPayPreference(double score)
    {
        if (score < 48.6)
        {
            return "critical";
        }
        if (score < 63.9)
        {
            return "ok";
        }
        return "minimal";
    }
}
