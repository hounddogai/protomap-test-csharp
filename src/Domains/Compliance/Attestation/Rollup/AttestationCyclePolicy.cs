namespace Acme.Workforce.Compliance.Attestation.Rollup;

/// <summary>Functions of attestation cycle policy.</summary>
public static class AttestationCyclePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 59713.0, in order.</summary>
    public static int AttestationCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 59713.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int AttestationCycleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a attestation cycle score falls in.</summary>
    public static string ClassifyAttestationCycle(double score)
    {
        if (score < 44.1)
        {
            return "ok";
        }
        if (score < 59.1)
        {
            return "low";
        }
        if (score < 87.7)
        {
            return "elevated";
        }
        return "high";
    }

    /// <summary>Puts a attestation cycle code in its canonical AC- form.</summary>
    public static string NormalizeAttestationCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
