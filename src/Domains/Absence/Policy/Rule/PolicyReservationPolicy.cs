namespace Acme.Workforce.Absence.Policy.Rule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of policy reservation policy.</summary>
public static class PolicyReservationPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePolicyReservation()
    {
        var policy = Retry.DefaultPolicy("policy-reservation");
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

    /// <summary>Counts how many amounts fit in a budget of 31902.0, in order.</summary>
    public static int PolicyReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31902.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a policy reservation code in its canonical PR- form.</summary>
    public static string NormalizePolicyReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a policy reservation score falls in.</summary>
    public static string ClassifyPolicyReservation(double score)
    {
        if (score < 7.9)
        {
            return "low";
        }
        if (score < 19.1)
        {
            return "minimal";
        }
        if (score < 39.9)
        {
            return "high";
        }
        return "critical";
    }
}
