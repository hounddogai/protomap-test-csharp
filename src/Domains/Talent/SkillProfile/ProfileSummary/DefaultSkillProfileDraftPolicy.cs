namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default skill profile draft policy.</summary>
public static class DefaultSkillProfileDraftPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultSkillProfileDraft()
    {
        var policy = Retry.DefaultPolicy("default-skill-profile-draft");
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

    public static string DefaultSkillProfileDraftBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
