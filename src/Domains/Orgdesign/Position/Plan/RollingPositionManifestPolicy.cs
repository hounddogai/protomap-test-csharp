namespace Acme.Workforce.Orgdesign.Position.Plan;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling position manifest policy.</summary>
public static class RollingPositionManifestPolicy
{
    /// <summary>Puts a rolling position manifest code in its canonical RPM- form.</summary>
    public static string NormalizeRollingPositionManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string RollingPositionManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingPositionManifest()
    {
        var policy = Retry.DefaultPolicy("rolling-position-manifest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static List<string> MergeRollingPositionManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
