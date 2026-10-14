namespace Acme.Workforce.Talent.Tests.SkillProfile.Schedule;

using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class DefaultSkillProfileScheduleChecks
{
    public static void DefaultSkillProfileScheduleStartsInProposed()
    {
        var item = DefaultSkillProfileSchedule.NewDefaultSkillProfileSchedule("code-114", "title-957");
        Check.Equal(item.Status, DefaultSkillProfileScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultSkillProfileScheduleMovesToApproved()
    {
        var item = DefaultSkillProfileSchedule.NewDefaultSkillProfileSchedule("code-114", "title-957");
        try
        {
            item.MoveTo(DefaultSkillProfileScheduleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultSkillProfileScheduleStatus.Approved, "status");
    }

    public static void DefaultSkillProfileScheduleRejectsMovesFromWithdrawn()
    {
        var item = DefaultSkillProfileSchedule.NewDefaultSkillProfileSchedule("code-114", "title-957");
        item.Status = DefaultSkillProfileScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultSkillProfileScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultSkillProfileScheduleStartsInProposed();
        DefaultSkillProfileScheduleMovesToApproved();
        DefaultSkillProfileScheduleRejectsMovesFromWithdrawn();
    }
}
