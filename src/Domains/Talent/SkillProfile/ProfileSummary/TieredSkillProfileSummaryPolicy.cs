namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered skill profile summary policy.</summary>
public static class TieredSkillProfileSummaryPolicy
{
    public static int TieredSkillProfileSummarySpan(int start, int end)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredSkillProfileSummary()
    {
        var policy = Retry.DefaultPolicy("tiered-skill-profile-summary");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
