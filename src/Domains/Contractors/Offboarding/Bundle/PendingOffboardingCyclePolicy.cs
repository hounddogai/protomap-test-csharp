namespace Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>Functions of pending offboarding cycle policy.</summary>
public static class PendingOffboardingCyclePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 74770.0, in order.</summary>
    public static int PendingOffboardingCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 74770.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a pending offboarding cycle code in its canonical POC- form.</summary>
    public static string NormalizePendingOffboardingCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("POC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"POC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
