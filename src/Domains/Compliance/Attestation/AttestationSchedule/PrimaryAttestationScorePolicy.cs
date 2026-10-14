namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary attestation score policy.</summary>
public static class PrimaryAttestationScorePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 31237.0, in order.</summary>
    public static int PrimaryAttestationScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31237.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryAttestationScore()
    {
        var policy = Retry.DefaultPolicy("primary-attestation-score");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a primary attestation score code in its canonical PAS- form.</summary>
    public static string NormalizePrimaryAttestationScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PrimaryAttestationScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
