namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunBundleChecks
{
    public static void PayRunBundleStartsInProposed()
    {
        var item = PayRunBundle.NewPayRunBundle("code-64", "title-646");
        Check.Equal(item.Status, PayRunBundleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunBundleMovesToRejected()
    {
        var item = PayRunBundle.NewPayRunBundle("code-64", "title-646");
        try
        {
            item.MoveTo(PayRunBundleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunBundleStatus.Rejected, "status");
    }

    public static void PayRunBundleRejectsMovesFromWithdrawn()
    {
        var item = PayRunBundle.NewPayRunBundle("code-64", "title-646");
        item.Status = PayRunBundleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayRunBundleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayRunBundleStartsInProposed();
        PayRunBundleMovesToRejected();
        PayRunBundleRejectsMovesFromWithdrawn();
    }
}
