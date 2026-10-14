namespace Acme.Workforce.Absence.Tests.Accrual.Baseline;

using Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AccrualBaselineChecks
{
    public static void AccrualBaselineStartsInProposed()
    {
        var item = AccrualBaseline.NewAccrualBaseline("code-100", "title-189");
        Check.Equal(item.Status, AccrualBaselineStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualBaselineMovesToRejected()
    {
        var item = AccrualBaseline.NewAccrualBaseline("code-100", "title-189");
        try
        {
            item.MoveTo(AccrualBaselineStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualBaselineStatus.Rejected, "status");
    }

    public static void AccrualBaselineRejectsMovesFromWithdrawn()
    {
        var item = AccrualBaseline.NewAccrualBaseline("code-100", "title-189");
        item.Status = AccrualBaselineStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AccrualBaselineStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AccrualBaselineStartsInProposed();
        AccrualBaselineMovesToRejected();
        AccrualBaselineRejectsMovesFromWithdrawn();
    }
}
