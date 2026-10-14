namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of span profile policy.</summary>
public static class SpanProfilePolicy
{
    public static double SpanProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.57;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string SpanProfileBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
