namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global span slot policy.</summary>
public static class GlobalSpanSlotPolicy
{
    /// <summary>Puts a global span slot code in its canonical GSS- form.</summary>
    public static string NormalizeGlobalSpanSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GSS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GSS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string GlobalSpanSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
