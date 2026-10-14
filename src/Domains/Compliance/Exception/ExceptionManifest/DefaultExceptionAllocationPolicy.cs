namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default exception allocation policy.</summary>
public static class DefaultExceptionAllocationPolicy
{
    public static double DefaultExceptionAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a default exception allocation score falls in.</summary>
    public static string ClassifyDefaultExceptionAllocation(double score)
    {
        if (score < 1.7)
        {
            return "high";
        }
        if (score < 16.0)
        {
            return "minimal";
        }
        if (score < 23.1)
        {
            return "critical";
        }
        return "ok";
    }
}
