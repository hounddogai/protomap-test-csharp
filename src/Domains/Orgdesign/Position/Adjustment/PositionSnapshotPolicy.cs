namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of position snapshot policy.</summary>
public static class PositionSnapshotPolicy
{
    public static List<string> MergePositionSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a position snapshot code in its canonical PS- form.</summary>
    public static string NormalizePositionSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int PositionSnapshotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }
}
