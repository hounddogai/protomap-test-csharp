namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of lenient certification plan policy.</summary>
public static class LenientCertificationPlanPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 16691.0, in order.</summary>
    public static int LenientCertificationPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 16691.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a lenient certification plan code in its canonical LCP- form.</summary>
    public static string NormalizeLenientCertificationPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LCP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LCP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientCertificationPlan()
    {
        var policy = Retry.DefaultPolicy("lenient-certification-plan");
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
