namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of retro pay override policy.</summary>
public static class RetroPayOverridePolicy
{
    public static double RetroPayOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.61;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int RetroPayOverrideSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static string RetroPayOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
