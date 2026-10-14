namespace Acme.Workforce.Talent.Tests.SkillProfile.Cycle;

using Acme.Workforce.Talent.SkillProfile.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScheduledSkillProfilePolicyChecks
{
    public static void ScheduledSkillProfilePolicyStartsInPlanned()
    {
        var item = ScheduledSkillProfilePolicy.NewScheduledSkillProfilePolicy("code-84", "label-310");
        Check.Equal(item.Status, ScheduledSkillProfilePolicyStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledSkillProfilePolicyMovesToScheduled()
    {
        var item = ScheduledSkillProfilePolicy.NewScheduledSkillProfilePolicy("code-84", "label-310");
        try
        {
            item.MoveTo(ScheduledSkillProfilePolicyStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledSkillProfilePolicyStatus.Scheduled, "status");
    }

    public static void ScheduledSkillProfilePolicyRejectsMovesFromCancelled()
    {
        var item = ScheduledSkillProfilePolicy.NewScheduledSkillProfilePolicy("code-84", "label-310");
        item.Status = ScheduledSkillProfilePolicyStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledSkillProfilePolicyStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledSkillProfilePolicyStartsInPlanned();
        ScheduledSkillProfilePolicyMovesToScheduled();
        ScheduledSkillProfilePolicyRejectsMovesFromCancelled();
    }
}
