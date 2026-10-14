namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient position profile policy.</summary>
public static class LenientPositionProfilePolicy
{
    public static int LenientPositionProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a lenient position profile code in its canonical LPP- form.</summary>
    public static string NormalizeLenientPositionProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LenientPositionProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.13;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
