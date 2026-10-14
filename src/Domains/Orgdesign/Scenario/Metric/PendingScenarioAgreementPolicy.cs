namespace Acme.Workforce.Orgdesign.Scenario.Metric;

/// <summary>Functions of pending scenario agreement policy.</summary>
public static class PendingScenarioAgreementPolicy
{
    /// <summary>Puts a pending scenario agreement code in its canonical PSA- form.</summary>
    public static string NormalizePendingScenarioAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a pending scenario agreement score falls in.</summary>
    public static string ClassifyPendingScenarioAgreement(double score)
    {
        if (score < 76.0)
        {
            return "watch";
        }
        if (score < 81.1)
        {
            return "minimal";
        }
        if (score < 84.3)
        {
            return "ok";
        }
        if (score < 98.4)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 39498.0, in order.</summary>
    public static int PendingScenarioAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 39498.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
