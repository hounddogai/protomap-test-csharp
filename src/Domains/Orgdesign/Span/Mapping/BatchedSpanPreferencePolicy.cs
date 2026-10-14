namespace Acme.Workforce.Orgdesign.Span.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of batched span preference policy.</summary>
public static class BatchedSpanPreferencePolicy
{
    public static string BatchedSpanPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double BatchedSpanPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.25;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
