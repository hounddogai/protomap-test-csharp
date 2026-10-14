namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of attestation exception policy.</summary>
public static class AttestationExceptionPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 7006.0, in order.</summary>
    public static int AttestationExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7006.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a attestation exception code in its canonical AE- form.</summary>
    public static string NormalizeAttestationExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAttestationException()
    {
        var policy = Retry.DefaultPolicy("attestation-exception");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
