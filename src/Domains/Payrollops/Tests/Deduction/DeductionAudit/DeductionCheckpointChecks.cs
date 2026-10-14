namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionAudit;

using Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Checks of deduction audit code, run by the project's runner.</summary>
public static class DeductionCheckpointChecks
{
    public static void DeductionCheckpointStartsInCreated()
    {
        var item = DeductionCheckpoint.NewDeductionCheckpoint("code-689", "name-547");
        Check.Equal(item.Status, DeductionCheckpointStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionCheckpointMovesToPublished()
    {
        var item = DeductionCheckpoint.NewDeductionCheckpoint("code-689", "name-547");
        try
        {
            item.MoveTo(DeductionCheckpointStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionCheckpointStatus.Published, "status");
    }

    public static void DeductionCheckpointRejectsMovesFromRetired()
    {
        var item = DeductionCheckpoint.NewDeductionCheckpoint("code-689", "name-547");
        item.Status = DeductionCheckpointStatus.Retired;
        Check.Fails(() => item.MoveTo(DeductionCheckpointStatus.Created), "move");
    }

    public static void RunAll()
    {
        DeductionCheckpointStartsInCreated();
        DeductionCheckpointMovesToPublished();
        DeductionCheckpointRejectsMovesFromRetired();
    }
}
