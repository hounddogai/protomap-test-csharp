namespace Acme.Workforce.Compensation.Tests.Adjustment.Template;

using Acme.Workforce.Compensation.Adjustment.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class RollingAdjustmentTemplateChecks
{
    public static void RollingAdjustmentTemplateStartsInCreated()
    {
        var item = RollingAdjustmentTemplate.NewRollingAdjustmentTemplate("code-107", "title-760");
        Check.Equal(item.Status, RollingAdjustmentTemplateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingAdjustmentTemplateMovesToValidated()
    {
        var item = RollingAdjustmentTemplate.NewRollingAdjustmentTemplate("code-107", "title-760");
        try
        {
            item.MoveTo(RollingAdjustmentTemplateStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingAdjustmentTemplateStatus.Validated, "status");
    }

    public static void RollingAdjustmentTemplateRejectsMovesFromRetired()
    {
        var item = RollingAdjustmentTemplate.NewRollingAdjustmentTemplate("code-107", "title-760");
        item.Status = RollingAdjustmentTemplateStatus.Retired;
        Check.Fails(() => item.MoveTo(RollingAdjustmentTemplateStatus.Created), "move");
    }

    public static void RunAll()
    {
        RollingAdjustmentTemplateStartsInCreated();
        RollingAdjustmentTemplateMovesToValidated();
        RollingAdjustmentTemplateRejectsMovesFromRetired();
    }
}
