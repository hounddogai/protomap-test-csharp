namespace Acme.Workforce.Payrollops.RetroPay.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of retro pay agreement policy.</summary>
public static class RetroPayAgreementPolicy
{
    public static string RetroPayAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double RetroPayAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.61;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a retro pay agreement score falls in.</summary>
    public static string ClassifyRetroPayAgreement(double score)
    {
        if (score < 7.0)
        {
            return "elevated";
        }
        if (score < 27.7)
        {
            return "medium";
        }
        if (score < 36.2)
        {
            return "low";
        }
        if (score < 83.5)
        {
            return "high";
        }
        return "watch";
    }

    public static int RetroPayAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
