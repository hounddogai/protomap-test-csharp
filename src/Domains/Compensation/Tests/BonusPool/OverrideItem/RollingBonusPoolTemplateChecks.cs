namespace Acme.Workforce.Compensation.Tests.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RollingBonusPoolTemplateChecks
{
    public static void RollingBonusPoolTemplateStartsInNew()
    {
        var item = RollingBonusPoolTemplate.NewRollingBonusPoolTemplate("code-995", "label-141");
        Check.Equal(item.Status, RollingBonusPoolTemplateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingBonusPoolTemplateMovesToRunning()
    {
        var item = RollingBonusPoolTemplate.NewRollingBonusPoolTemplate("code-995", "label-141");
        try
        {
            item.MoveTo(RollingBonusPoolTemplateStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingBonusPoolTemplateStatus.Running, "status");
    }

    public static void RollingBonusPoolTemplateRejectsMovesFromFailed()
    {
        var item = RollingBonusPoolTemplate.NewRollingBonusPoolTemplate("code-995", "label-141");
        item.Status = RollingBonusPoolTemplateStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingBonusPoolTemplateStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingBonusPoolTemplateStartsInNew();
        RollingBonusPoolTemplateMovesToRunning();
        RollingBonusPoolTemplateRejectsMovesFromFailed();
    }
}
