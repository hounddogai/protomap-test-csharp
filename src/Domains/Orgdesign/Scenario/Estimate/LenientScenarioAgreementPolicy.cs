namespace Acme.Workforce.Orgdesign.Scenario.Estimate;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient scenario agreement policy.</summary>
public static class LenientScenarioAgreementPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientScenarioAgreement()
    {
        var policy = Retry.DefaultPolicy("lenient-scenario-agreement");
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

    /// <summary>Counts how many amounts fit in a budget of 56487.0, in order.</summary>
    public static int LenientScenarioAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56487.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a lenient scenario agreement code in its canonical LSA- form.</summary>
    public static string NormalizeLenientScenarioAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a lenient scenario agreement score falls in.</summary>
    public static string ClassifyLenientScenarioAgreement(double score)
    {
        if (score < 17.6)
        {
            return "minimal";
        }
        if (score < 75.4)
        {
            return "high";
        }
        if (score < 92.9)
        {
            return "medium";
        }
        return "watch";
    }
}
