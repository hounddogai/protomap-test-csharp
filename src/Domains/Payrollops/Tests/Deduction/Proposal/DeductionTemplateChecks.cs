namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeductionTemplateChecks
{
    public static void DeductionTemplateStartsInIdle()
    {
        var item = DeductionTemplate.NewDeductionTemplate("code-71", "label-787");
        Check.Equal(item.Status, DeductionTemplateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionTemplateMovesToDraining()
    {
        var item = DeductionTemplate.NewDeductionTemplate("code-71", "label-787");
        try
        {
            item.MoveTo(DeductionTemplateStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionTemplateStatus.Draining, "status");
    }

    public static void DeductionTemplateRejectsMovesFromStopped()
    {
        var item = DeductionTemplate.NewDeductionTemplate("code-71", "label-787");
        item.Status = DeductionTemplateStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeductionTemplateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeductionTemplateStartsInIdle();
        DeductionTemplateMovesToDraining();
        DeductionTemplateRejectsMovesFromStopped();
    }
}
