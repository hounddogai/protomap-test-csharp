namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class PrimaryEquityGrantDigestChecks
{
    public static void PrimaryEquityGrantDigestStartsInDraft()
    {
        var item = PrimaryEquityGrantDigest.NewPrimaryEquityGrantDigest("code-923", "name-499");
        Check.Equal(item.Status, PrimaryEquityGrantDigestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryEquityGrantDigestMovesToPending()
    {
        var item = PrimaryEquityGrantDigest.NewPrimaryEquityGrantDigest("code-923", "name-499");
        try
        {
            item.MoveTo(PrimaryEquityGrantDigestStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryEquityGrantDigestStatus.Pending, "status");
    }

    public static void PrimaryEquityGrantDigestRejectsMovesFromClosed()
    {
        var item = PrimaryEquityGrantDigest.NewPrimaryEquityGrantDigest("code-923", "name-499");
        item.Status = PrimaryEquityGrantDigestStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryEquityGrantDigestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryEquityGrantDigestStartsInDraft();
        PrimaryEquityGrantDigestMovesToPending();
        PrimaryEquityGrantDigestRejectsMovesFromClosed();
    }
}
