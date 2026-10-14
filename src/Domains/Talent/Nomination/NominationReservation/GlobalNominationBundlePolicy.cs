namespace Acme.Workforce.Talent.Nomination.NominationReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global nomination bundle policy.</summary>
public static class GlobalNominationBundlePolicy
{
    public static string GlobalNominationBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int GlobalNominationBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Puts a global nomination bundle code in its canonical GNB- form.</summary>
    public static string NormalizeGlobalNominationBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GNB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GNB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 93982.0, in order.</summary>
    public static int GlobalNominationBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93982.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
