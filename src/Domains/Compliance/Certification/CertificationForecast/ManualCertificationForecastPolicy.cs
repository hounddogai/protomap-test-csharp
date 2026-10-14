namespace Acme.Workforce.Compliance.Certification.CertificationForecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual certification forecast policy.</summary>
public static class ManualCertificationForecastPolicy
{
    public static string ManualCertificationForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a manual certification forecast score falls in.</summary>
    public static string ClassifyManualCertificationForecast(double score)
    {
        if (score < 6.0)
        {
            return "low";
        }
        if (score < 29.5)
        {
            return "medium";
        }
        return "critical";
    }
}
