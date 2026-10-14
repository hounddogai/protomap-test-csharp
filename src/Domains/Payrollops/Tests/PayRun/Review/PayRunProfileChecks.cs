namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunProfileChecks
{
    public static void PayRunProfileStartsInProposed()
    {
        var item = PayRunProfile.NewPayRunProfile("code-378", "title-978");
        Check.Equal(item.Status, PayRunProfileStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunProfileMovesToRejected()
    {
        var item = PayRunProfile.NewPayRunProfile("code-378", "title-978");
        try
        {
            item.MoveTo(PayRunProfileStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunProfileStatus.Rejected, "status");
    }

    public static void PayRunProfileRejectsMovesFromWithdrawn()
    {
        var item = PayRunProfile.NewPayRunProfile("code-378", "title-978");
        item.Status = PayRunProfileStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayRunProfileStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayRunProfileStartsInProposed();
        PayRunProfileMovesToRejected();
        PayRunProfileRejectsMovesFromWithdrawn();
    }
}
