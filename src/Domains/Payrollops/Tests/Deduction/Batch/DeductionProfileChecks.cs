namespace Acme.Workforce.Payrollops.Tests.Deduction.Batch;

using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DeductionProfileChecks
{
    public static void DeductionProfileStartsInOpen()
    {
        var item = DeductionProfile.NewDeductionProfile("code-798", "title-392");
        Check.Equal(item.Status, DeductionProfileStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionProfileMovesToReopened()
    {
        var item = DeductionProfile.NewDeductionProfile("code-798", "title-392");
        try
        {
            item.MoveTo(DeductionProfileStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionProfileStatus.Reopened, "status");
    }

    public static void DeductionProfileRejectsMovesFromArchived()
    {
        var item = DeductionProfile.NewDeductionProfile("code-798", "title-392");
        item.Status = DeductionProfileStatus.Archived;
        Check.Fails(() => item.MoveTo(DeductionProfileStatus.Open), "move");
    }

    public static void RunAll()
    {
        DeductionProfileStartsInOpen();
        DeductionProfileMovesToReopened();
        DeductionProfileRejectsMovesFromArchived();
    }
}
