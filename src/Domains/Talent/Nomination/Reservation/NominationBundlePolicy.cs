namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of nomination bundle policy.</summary>
public static class NominationBundlePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 34575.0, in order.</summary>
    public static int NominationBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 34575.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a nomination bundle code in its canonical NB- form.</summary>
    public static string NormalizeNominationBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("NB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"NB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double NominationBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.94;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
