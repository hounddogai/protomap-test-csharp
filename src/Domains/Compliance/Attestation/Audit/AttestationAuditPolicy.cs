namespace Acme.Workforce.Compliance.Attestation.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation audit policy.</summary>
public static class AttestationAuditPolicy
{
    /// <summary>Names the band a attestation audit score falls in.</summary>
    public static string ClassifyAttestationAudit(double score)
    {
        if (score < 25.5)
        {
            return "high";
        }
        if (score < 45.2)
        {
            return "low";
        }
        if (score < 59.0)
        {
            return "ok";
        }
        return "minimal";
    }

    public static double AttestationAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string AttestationAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
