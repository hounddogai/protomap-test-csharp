namespace Acme.Workforce.Compliance.Certification.CertificationAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered certification summary policy.</summary>
public static class TieredCertificationSummaryPolicy
{
    /// <summary>Names the band a tiered certification summary score falls in.</summary>
    public static string ClassifyTieredCertificationSummary(double score)
    {
        if (score < 24.0)
        {
            return "low";
        }
        if (score < 38.8)
        {
            return "medium";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 9640.0, in order.</summary>
    public static int TieredCertificationSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9640.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double TieredCertificationSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
