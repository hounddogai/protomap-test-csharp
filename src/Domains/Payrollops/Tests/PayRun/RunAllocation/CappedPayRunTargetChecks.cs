namespace Acme.Workforce.Payrollops.Tests.PayRun.RunAllocation;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Checks of run allocation code, run by the project's runner.</summary>
public static class CappedPayRunTargetChecks
{
    public static void CappedPayRunTargetStartsInRequested()
    {
        var item = CappedPayRunTarget.NewCappedPayRunTarget("code-538", "title-233");
        Check.Equal(item.Status, CappedPayRunTargetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPayRunTargetMovesToRevoked()
    {
        var item = CappedPayRunTarget.NewCappedPayRunTarget("code-538", "title-233");
        try
        {
            item.MoveTo(CappedPayRunTargetStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPayRunTargetStatus.Revoked, "status");
    }

    public static void CappedPayRunTargetRejectsMovesFromExpired()
    {
        var item = CappedPayRunTarget.NewCappedPayRunTarget("code-538", "title-233");
        item.Status = CappedPayRunTargetStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedPayRunTargetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedPayRunTargetStartsInRequested();
        CappedPayRunTargetMovesToRevoked();
        CappedPayRunTargetRejectsMovesFromExpired();
    }
}
