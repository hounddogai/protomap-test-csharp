namespace Acme.Workforce.Compliance.Certification.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of certification bundle policy.</summary>
public static class CertificationBundlePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 50864.0, in order.</summary>
    public static int CertificationBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50864.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double CertificationBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.34;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a certification bundle score falls in.</summary>
    public static string ClassifyCertificationBundle(double score)
    {
        if (score < 47.2)
        {
            return "high";
        }
        if (score < 70.3)
        {
            return "low";
        }
        if (score < 96.7)
        {
            return "minimal";
        }
        if (score < 98.0)
        {
            return "elevated";
        }
        return "watch";
    }

    /// <summary>Puts a certification bundle code in its canonical CB- form.</summary>
    public static string NormalizeCertificationBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
