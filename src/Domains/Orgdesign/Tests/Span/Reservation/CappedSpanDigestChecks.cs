namespace Acme.Workforce.Orgdesign.Tests.Span.Reservation;

using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedSpanDigestChecks
{
    public static void CappedSpanDigestStartsInDraft()
    {
        var item = CappedSpanDigest.NewCappedSpanDigest("code-649", "title-4");
        Check.Equal(item.Status, CappedSpanDigestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedSpanDigestMovesToActive()
    {
        var item = CappedSpanDigest.NewCappedSpanDigest("code-649", "title-4");
        try
        {
            item.MoveTo(CappedSpanDigestStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedSpanDigestStatus.Active, "status");
    }

    public static void CappedSpanDigestRejectsMovesFromClosed()
    {
        var item = CappedSpanDigest.NewCappedSpanDigest("code-649", "title-4");
        item.Status = CappedSpanDigestStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedSpanDigestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedSpanDigestStartsInDraft();
        CappedSpanDigestMovesToActive();
        CappedSpanDigestRejectsMovesFromClosed();
    }
}
