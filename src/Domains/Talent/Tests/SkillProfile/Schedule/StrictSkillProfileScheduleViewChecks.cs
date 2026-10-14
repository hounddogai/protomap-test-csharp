namespace Acme.Workforce.Talent.Tests.SkillProfile.Schedule;

using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class StrictSkillProfileScheduleViewChecks
{
    public static void StrictSkillProfileScheduleViewShowsTheStatusInCapitals()
    {
        var item = StrictSkillProfileSchedule.NewStrictSkillProfileSchedule("code-50", "name-877");
        var view = StrictSkillProfileScheduleView.ToStrictSkillProfileScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictSkillProfileScheduleView.ToStrictSkillProfileScheduleViews(new List<StrictSkillProfileSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictSkillProfileScheduleViewShowsTheStatusInCapitals();
    }
}
