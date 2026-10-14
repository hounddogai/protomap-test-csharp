namespace Acme.Workforce.Orgdesign.Position.Audit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of position audit policy.</summary>
public static class PositionAuditPolicy
{
    public static int PositionAuditSpan(int start, int end)
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
        return span / 60;
    }

    public static string PositionAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a position audit code in its canonical PA- form.</summary>
    public static string NormalizePositionAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePositionAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
