namespace Acme.Workforce.Absence.Tests.ReturnPlan.Template;

using Acme.Workforce.Absence.ReturnPlan.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ReturnPlanTemplateChecks
{
    public static void ReturnPlanTemplateStartsInCreated()
    {
        var item = ReturnPlanTemplate.NewReturnPlanTemplate("code-731", "label-35");
        Check.Equal(item.Status, ReturnPlanTemplateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanTemplateMovesToPublished()
    {
        var item = ReturnPlanTemplate.NewReturnPlanTemplate("code-731", "label-35");
        try
        {
            item.MoveTo(ReturnPlanTemplateStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanTemplateStatus.Published, "status");
    }

    public static void ReturnPlanTemplateRejectsMovesFromRetired()
    {
        var item = ReturnPlanTemplate.NewReturnPlanTemplate("code-731", "label-35");
        item.Status = ReturnPlanTemplateStatus.Retired;
        Check.Fails(() => item.MoveTo(ReturnPlanTemplateStatus.Created), "move");
    }

    public static void RunAll()
    {
        ReturnPlanTemplateStartsInCreated();
        ReturnPlanTemplateMovesToPublished();
        ReturnPlanTemplateRejectsMovesFromRetired();
    }
}
