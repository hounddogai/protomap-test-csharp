namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualWindowChecks
{
    public static void AccrualWindowStartsInRequested()
    {
        var item = AccrualWindow.NewAccrualWindow("code-890", "title-660");
        Check.Equal(item.Status, AccrualWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualWindowMovesToRevoked()
    {
        var item = AccrualWindow.NewAccrualWindow("code-890", "title-660");
        try
        {
            item.MoveTo(AccrualWindowStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualWindowStatus.Revoked, "status");
    }

    public static void AccrualWindowRejectsMovesFromExpired()
    {
        var item = AccrualWindow.NewAccrualWindow("code-890", "title-660");
        item.Status = AccrualWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(AccrualWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AccrualWindowStartsInRequested();
        AccrualWindowMovesToRevoked();
        AccrualWindowRejectsMovesFromExpired();
    }
}
