namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of offboarding bundle policy.</summary>
public static class OffboardingBundlePolicy
{
    public static string OffboardingBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a offboarding bundle score falls in.</summary>
    public static string ClassifyOffboardingBundle(double score)
    {
        if (score < 21.0)
        {
            return "medium";
        }
        if (score < 42.9)
        {
            return "minimal";
        }
        if (score < 58.5)
        {
            return "critical";
        }
        if (score < 76.3)
        {
            return "ok";
        }
        return "low";
    }
}
