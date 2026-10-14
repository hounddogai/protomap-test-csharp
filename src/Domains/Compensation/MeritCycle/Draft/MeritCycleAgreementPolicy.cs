namespace Acme.Workforce.Compensation.MeritCycle.Draft;

/// <summary>Functions of merit cycle agreement policy.</summary>
public static class MeritCycleAgreementPolicy
{
    /// <summary>Puts a merit cycle agreement code in its canonical MCA- form.</summary>
    public static string NormalizeMeritCycleAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MCA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MCA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a merit cycle agreement score falls in.</summary>
    public static string ClassifyMeritCycleAgreement(double score)
    {
        if (score < 7.1)
        {
            return "high";
        }
        if (score < 11.9)
        {
            return "ok";
        }
        if (score < 14.3)
        {
            return "elevated";
        }
        if (score < 60.3)
        {
            return "watch";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 79060.0, in order.</summary>
    public static int MeritCycleAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 79060.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
