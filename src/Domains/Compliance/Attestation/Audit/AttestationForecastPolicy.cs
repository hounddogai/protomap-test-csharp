namespace Acme.Workforce.Compliance.Attestation.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of attestation forecast policy.</summary>
public static class AttestationForecastPolicy
{
    public static double AttestationForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.15;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a attestation forecast score falls in.</summary>
    public static string ClassifyAttestationForecast(double score)
    {
        if (score < 22.9)
        {
            return "critical";
        }
        if (score < 31.8)
        {
            return "high";
        }
        if (score < 56.9)
        {
            return "minimal";
        }
        if (score < 70.4)
        {
            return "watch";
        }
        return "ok";
    }
}
