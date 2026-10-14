namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive skill profile snapshot policy.</summary>
public static class AdaptiveSkillProfileSnapshotPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveSkillProfileSnapshot()
    {
        var policy = Retry.DefaultPolicy("adaptive-skill-profile-snapshot");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int AdaptiveSkillProfileSnapshotSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a adaptive skill profile snapshot code in its canonical ASPS- form.</summary>
    public static string NormalizeAdaptiveSkillProfileSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ASPS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ASPS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
