namespace Acme.Workforce.Talent.SkillProfile.Cycle;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled skill profile policy policy.</summary>
public static class ScheduledSkillProfilePolicyPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 8462.0, in order.</summary>
    public static int ScheduledSkillProfilePolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8462.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledSkillProfilePolicy()
    {
        var policy = Retry.DefaultPolicy("scheduled-skill-profile-policy");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
