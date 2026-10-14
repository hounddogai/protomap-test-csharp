namespace Acme.Workforce.Assets.Assignment.Rule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of assignment rule policy.</summary>
public static class AssignmentRulePolicy
{
    /// <summary>Names the band a assignment rule score falls in.</summary>
    public static string ClassifyAssignmentRule(double score)
    {
        if (score < 2.1)
        {
            return "ok";
        }
        if (score < 15.6)
        {
            return "watch";
        }
        if (score < 70.5)
        {
            return "high";
        }
        return "low";
    }

    /// <summary>Puts a assignment rule code in its canonical AR- form.</summary>
    public static string NormalizeAssignmentRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssignmentRule()
    {
        var policy = Retry.DefaultPolicy("assignment-rule");
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
}
