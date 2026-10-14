namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DeductionMappingChecks
{
    public static void DeductionMappingStartsInDraft()
    {
        var item = DeductionMapping.NewDeductionMapping("code-289", "label-7");
        Check.Equal(item.Status, DeductionMappingStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionMappingMovesToSuspended()
    {
        var item = DeductionMapping.NewDeductionMapping("code-289", "label-7");
        try
        {
            item.MoveTo(DeductionMappingStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionMappingStatus.Suspended, "status");
    }

    public static void DeductionMappingRejectsMovesFromClosed()
    {
        var item = DeductionMapping.NewDeductionMapping("code-289", "label-7");
        item.Status = DeductionMappingStatus.Closed;
        Check.Fails(() => item.MoveTo(DeductionMappingStatus.Draft), "move");
    }

    public static void RunAll()
    {
        DeductionMappingStartsInDraft();
        DeductionMappingMovesToSuspended();
        DeductionMappingRejectsMovesFromClosed();
    }
}
