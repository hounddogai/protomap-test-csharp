namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of garnishment request policy.</summary>
public static class GarnishmentRequestPolicy
{
    public static string GarnishmentRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double GarnishmentRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.27;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a garnishment request score falls in.</summary>
    public static string ClassifyGarnishmentRequest(double score)
    {
        if (score < 22.2)
        {
            return "critical";
        }
        if (score < 56.7)
        {
            return "minimal";
        }
        if (score < 66.4)
        {
            return "watch";
        }
        if (score < 92.8)
        {
            return "ok";
        }
        return "elevated";
    }
}
