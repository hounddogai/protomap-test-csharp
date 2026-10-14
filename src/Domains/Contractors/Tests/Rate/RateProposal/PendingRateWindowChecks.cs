namespace Acme.Workforce.Contractors.Tests.Rate.RateProposal;

using Acme.Workforce.Contractors.Rate.RateProposal;

/// <summary>Checks of rate proposal code, run by the project's runner.</summary>
public static class PendingRateWindowChecks
{
    public static void PendingRateWindowStartsInCreated()
    {
        var item = PendingRateWindow.NewPendingRateWindow("code-805", "title-100");
        Check.Equal(item.Status, PendingRateWindowStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingRateWindowMovesToValidated()
    {
        var item = PendingRateWindow.NewPendingRateWindow("code-805", "title-100");
        try
        {
            item.MoveTo(PendingRateWindowStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingRateWindowStatus.Validated, "status");
    }

    public static void PendingRateWindowRejectsMovesFromRetired()
    {
        var item = PendingRateWindow.NewPendingRateWindow("code-805", "title-100");
        item.Status = PendingRateWindowStatus.Retired;
        Check.Fails(() => item.MoveTo(PendingRateWindowStatus.Created), "move");
    }

    public static void RunAll()
    {
        PendingRateWindowStartsInCreated();
        PendingRateWindowMovesToValidated();
        PendingRateWindowRejectsMovesFromRetired();
    }
}
