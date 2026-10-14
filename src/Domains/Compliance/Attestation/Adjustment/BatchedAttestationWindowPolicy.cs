namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched attestation window policy.</summary>
public static class BatchedAttestationWindowPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 91069.0, in order.</summary>
    public static int BatchedAttestationWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 91069.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a batched attestation window code in its canonical BAW- form.</summary>
    public static string NormalizeBatchedAttestationWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BAW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BAW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedAttestationWindow()
    {
        var policy = Retry.DefaultPolicy("batched-attestation-window");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
