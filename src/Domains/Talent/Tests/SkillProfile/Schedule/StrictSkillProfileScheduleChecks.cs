namespace Acme.Workforce.Talent.Tests.SkillProfile.Schedule;

using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class StrictSkillProfileScheduleChecks
{
    public static void StrictSkillProfileScheduleStartsInDraft()
    {
        var item = StrictSkillProfileSchedule.NewStrictSkillProfileSchedule("code-50", "name-877");
        Check.Equal(item.Status, StrictSkillProfileScheduleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictSkillProfileScheduleMovesToActive()
    {
        var item = StrictSkillProfileSchedule.NewStrictSkillProfileSchedule("code-50", "name-877");
        try
        {
            item.MoveTo(StrictSkillProfileScheduleStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictSkillProfileScheduleStatus.Active, "status");
    }

    public static void StrictSkillProfileScheduleRejectsMovesFromClosed()
    {
        var item = StrictSkillProfileSchedule.NewStrictSkillProfileSchedule("code-50", "name-877");
        item.Status = StrictSkillProfileScheduleStatus.Closed;
        Check.Fails(() => item.MoveTo(StrictSkillProfileScheduleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StrictSkillProfileScheduleStartsInDraft();
        StrictSkillProfileScheduleMovesToActive();
        StrictSkillProfileScheduleRejectsMovesFromClosed();
    }
}
