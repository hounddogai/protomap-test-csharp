namespace Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy offboarding request policy.</summary>
public static class LegacyOffboardingRequestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyOffboardingRequest()
    {
        var policy = Retry.DefaultPolicy("legacy-offboarding-request");
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

    /// <summary>Puts a legacy offboarding request code in its canonical LOR- form.</summary>
    public static string NormalizeLegacyOffboardingRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LOR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LOR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a legacy offboarding request score falls in.</summary>
    public static string ClassifyLegacyOffboardingRequest(double score)
    {
        if (score < 24.5)
        {
            return "medium";
        }
        if (score < 69.3)
        {
            return "elevated";
        }
        if (score < 87.6)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 85127.0, in order.</summary>
    public static int LegacyOffboardingRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 85127.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
