namespace Acme.Workforce.Talent.Tests.SkillProfile.Adjustment;

using Acme.Workforce.Talent.SkillProfile.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SkillProfileEstimateChecks
{
    public static void SkillProfileEstimateStartsInPlanned()
    {
        var item = SkillProfileEstimate.NewSkillProfileEstimate("code-561", "label-31");
        Check.Equal(item.Status, SkillProfileEstimateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SkillProfileEstimateMovesToScheduled()
    {
        var item = SkillProfileEstimate.NewSkillProfileEstimate("code-561", "label-31");
        try
        {
            item.MoveTo(SkillProfileEstimateStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SkillProfileEstimateStatus.Scheduled, "status");
    }

    public static void SkillProfileEstimateRejectsMovesFromCancelled()
    {
        var item = SkillProfileEstimate.NewSkillProfileEstimate("code-561", "label-31");
        item.Status = SkillProfileEstimateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SkillProfileEstimateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SkillProfileEstimateStartsInPlanned();
        SkillProfileEstimateMovesToScheduled();
        SkillProfileEstimateRejectsMovesFromCancelled();
    }
}
