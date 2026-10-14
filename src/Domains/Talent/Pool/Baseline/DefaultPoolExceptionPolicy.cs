namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default pool exception policy.</summary>
public static class DefaultPoolExceptionPolicy
{
    /// <summary>Puts a default pool exception code in its canonical DPE- form.</summary>
    public static string NormalizeDefaultPoolExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double DefaultPoolExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.62;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeDefaultPoolExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int DefaultPoolExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }
}
