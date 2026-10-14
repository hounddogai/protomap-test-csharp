namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary asset return profile policy.</summary>
public static class PrimaryAssetReturnProfilePolicy
{
    public static int PrimaryAssetReturnProfileSpan(int start, int end)
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

    /// <summary>Puts a primary asset return profile code in its canonical PARP- form.</summary>
    public static string NormalizePrimaryAssetReturnProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PARP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PARP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePrimaryAssetReturnProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
