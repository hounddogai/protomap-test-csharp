namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary certification bundle policy.</summary>
public static class PrimaryCertificationBundlePolicy
{
    /// <summary>Names the band a primary certification bundle score falls in.</summary>
    public static string ClassifyPrimaryCertificationBundle(double score)
    {
        if (score < 31.0)
        {
            return "low";
        }
        if (score < 35.1)
        {
            return "critical";
        }
        if (score < 72.4)
        {
            return "minimal";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 89185.0, in order.</summary>
    public static int PrimaryCertificationBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89185.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PrimaryCertificationBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.59;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
