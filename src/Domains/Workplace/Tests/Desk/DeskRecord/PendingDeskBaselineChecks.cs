namespace Acme.Workforce.Workplace.Tests.Desk.DeskRecord;

using Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>Checks of desk record code, run by the project's runner.</summary>
public static class PendingDeskBaselineChecks
{
    public static void PendingDeskBaselineStartsInRequested()
    {
        var item = PendingDeskBaseline.NewPendingDeskBaseline("code-666", "name-638");
        Check.Equal(item.Status, PendingDeskBaselineStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingDeskBaselineMovesToRevoked()
    {
        var item = PendingDeskBaseline.NewPendingDeskBaseline("code-666", "name-638");
        try
        {
            item.MoveTo(PendingDeskBaselineStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingDeskBaselineStatus.Revoked, "status");
    }

    public static void PendingDeskBaselineRejectsMovesFromExpired()
    {
        var item = PendingDeskBaseline.NewPendingDeskBaseline("code-666", "name-638");
        item.Status = PendingDeskBaselineStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingDeskBaselineStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingDeskBaselineStartsInRequested();
        PendingDeskBaselineMovesToRevoked();
        PendingDeskBaselineRejectsMovesFromExpired();
    }
}
