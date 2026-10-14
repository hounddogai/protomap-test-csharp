namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of span slot policy.</summary>
public static class SpanSlotPolicy
{
    public static int SpanSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string SpanSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double SpanSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a span slot code in its canonical SS- form.</summary>
    public static string NormalizeSpanSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
