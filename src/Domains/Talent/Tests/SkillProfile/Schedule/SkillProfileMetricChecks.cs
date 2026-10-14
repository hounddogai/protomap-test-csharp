namespace Acme.Workforce.Talent.Tests.SkillProfile.Schedule;

using Acme.Workforce.Talent.SkillProfile.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class SkillProfileMetricChecks
{
    public static void SkillProfileMetricStartsInPlanned()
    {
        var item = SkillProfileMetric.NewSkillProfileMetric("code-371", "title-812");
        Check.Equal(item.Status, SkillProfileMetricStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileMetricMovesToScheduled()
    {
        var item = SkillProfileMetric.NewSkillProfileMetric("code-371", "title-812");
        try
        {
            item.MoveTo(SkillProfileMetricStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileMetricStatus.Scheduled, "status");
    }

    public static void SkillProfileMetricRejectsMovesFromCancelled()
    {
        var item = SkillProfileMetric.NewSkillProfileMetric("code-371", "title-812");
        item.Status = SkillProfileMetricStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SkillProfileMetricStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SkillProfileMetricStartsInPlanned();
        SkillProfileMetricMovesToScheduled();
        SkillProfileMetricRejectsMovesFromCancelled();
    }
}
