namespace Acme.Workforce.Payrollops.Tests.PayRun.RunLedger;

using Acme.Workforce.Payrollops.PayRun.RunLedger;

/// <summary>Checks of run ledger code, run by the project's runner.</summary>
public static class PayRunPreferenceChecks
{
    public static void PayRunPreferenceStartsInProposed()
    {
        var item = PayRunPreference.NewPayRunPreference("code-50", "title-435");
        Check.Equal(item.Status, PayRunPreferenceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunPreferenceMovesToApproved()
    {
        var item = PayRunPreference.NewPayRunPreference("code-50", "title-435");
        try
        {
            item.MoveTo(PayRunPreferenceStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunPreferenceStatus.Approved, "status");
    }

    public static void PayRunPreferenceRejectsMovesFromWithdrawn()
    {
        var item = PayRunPreference.NewPayRunPreference("code-50", "title-435");
        item.Status = PayRunPreferenceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PayRunPreferenceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PayRunPreferenceStartsInProposed();
        PayRunPreferenceMovesToApproved();
        PayRunPreferenceRejectsMovesFromWithdrawn();
    }
}
