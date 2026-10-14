namespace Acme.Workforce.Compliance.Certification.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of certification exception policy.</summary>
public static class CertificationExceptionPolicy
{
    public static double CertificationExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.18;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a certification exception score falls in.</summary>
    public static string ClassifyCertificationException(double score)
    {
        if (score < 15.7)
        {
            return "high";
        }
        if (score < 84.2)
        {
            return "elevated";
        }
        return "critical";
    }
}
