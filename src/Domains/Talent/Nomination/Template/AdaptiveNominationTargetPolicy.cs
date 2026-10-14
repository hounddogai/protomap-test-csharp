namespace Acme.Workforce.Talent.Nomination.Template;

/// <summary>Functions of adaptive nomination target policy.</summary>
public static class AdaptiveNominationTargetPolicy
{
    /// <summary>Puts a adaptive nomination target code in its canonical ANT- form.</summary>
    public static string NormalizeAdaptiveNominationTargetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ANT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ANT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 64329.0, in order.</summary>
    public static int AdaptiveNominationTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64329.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
