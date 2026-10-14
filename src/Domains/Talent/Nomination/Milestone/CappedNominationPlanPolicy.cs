namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped nomination plan policy.</summary>
public static class CappedNominationPlanPolicy
{
    /// <summary>Puts a capped nomination plan code in its canonical CNP- form.</summary>
    public static string NormalizeCappedNominationPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CNP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CNP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string CappedNominationPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedNominationPlan()
    {
        var policy = Retry.DefaultPolicy("capped-nomination-plan");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a capped nomination plan score falls in.</summary>
    public static string ClassifyCappedNominationPlan(double score)
    {
        if (score < 26.3)
        {
            return "watch";
        }
        if (score < 49.7)
        {
            return "low";
        }
        return "medium";
    }
}
