namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RetroPayDigestChecks
{
    public static void RetroPayDigestStartsInDraft()
    {
        var item = RetroPayDigest.NewRetroPayDigest("code-97", "title-609");
        Check.Equal(item.Status, RetroPayDigestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayDigestMovesToSuspended()
    {
        var item = RetroPayDigest.NewRetroPayDigest("code-97", "title-609");
        try
        {
            item.MoveTo(RetroPayDigestStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayDigestStatus.Suspended, "status");
    }

    public static void RetroPayDigestRejectsMovesFromClosed()
    {
        var item = RetroPayDigest.NewRetroPayDigest("code-97", "title-609");
        item.Status = RetroPayDigestStatus.Closed;
        Check.Fails(() => item.MoveTo(RetroPayDigestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RetroPayDigestStartsInDraft();
        RetroPayDigestMovesToSuspended();
        RetroPayDigestRejectsMovesFromClosed();
    }
}
