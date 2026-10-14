namespace Acme.Workforce.Assets.LicenseSeat.Checkpoint;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of license seat rule policy.</summary>
public static class LicenseSeatRulePolicy
{
    /// <summary>Names the band a license seat rule score falls in.</summary>
    public static string ClassifyLicenseSeatRule(double score)
    {
        if (score < 25.8)
        {
            return "medium";
        }
        if (score < 32.7)
        {
            return "minimal";
        }
        if (score < 80.5)
        {
            return "watch";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLicenseSeatRule()
    {
        var policy = Retry.DefaultPolicy("license-seat-rule");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a license seat rule code in its canonical LSR- form.</summary>
    public static string NormalizeLicenseSeatRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 76527.0, in order.</summary>
    public static int LicenseSeatRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 76527.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
