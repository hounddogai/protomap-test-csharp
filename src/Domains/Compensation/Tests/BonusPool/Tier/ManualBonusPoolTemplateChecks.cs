namespace Acme.Workforce.Compensation.Tests.BonusPool.Tier;

using Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualBonusPoolTemplateChecks
{
    public static void ManualBonusPoolTemplateStartsInPlanned()
    {
        var item = ManualBonusPoolTemplate.NewManualBonusPoolTemplate("code-426", "title-743");
        Check.Equal(item.Status, ManualBonusPoolTemplateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualBonusPoolTemplateMovesToInProgress()
    {
        var item = ManualBonusPoolTemplate.NewManualBonusPoolTemplate("code-426", "title-743");
        try
        {
            item.MoveTo(ManualBonusPoolTemplateStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualBonusPoolTemplateStatus.InProgress, "status");
    }

    public static void ManualBonusPoolTemplateRejectsMovesFromCancelled()
    {
        var item = ManualBonusPoolTemplate.NewManualBonusPoolTemplate("code-426", "title-743");
        item.Status = ManualBonusPoolTemplateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ManualBonusPoolTemplateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ManualBonusPoolTemplateStartsInPlanned();
        ManualBonusPoolTemplateMovesToInProgress();
        ManualBonusPoolTemplateRejectsMovesFromCancelled();
    }
}
