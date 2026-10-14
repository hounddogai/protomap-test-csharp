namespace Acme.Workforce.Compensation.Tests.EquityGrant.Adjustment;

using Acme.Workforce.Compensation.EquityGrant.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EquityGrantRequestChecks
{
    public static void EquityGrantRequestStartsInDraft()
    {
        var item = EquityGrantRequest.NewEquityGrantRequest("code-477", "title-792");
        Check.Equal(item.Status, EquityGrantRequestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantRequestMovesToPending()
    {
        var item = EquityGrantRequest.NewEquityGrantRequest("code-477", "title-792");
        try
        {
            item.MoveTo(EquityGrantRequestStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantRequestStatus.Pending, "status");
    }

    public static void EquityGrantRequestRejectsMovesFromClosed()
    {
        var item = EquityGrantRequest.NewEquityGrantRequest("code-477", "title-792");
        item.Status = EquityGrantRequestStatus.Closed;
        Check.Fails(() => item.MoveTo(EquityGrantRequestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        EquityGrantRequestStartsInDraft();
        EquityGrantRequestMovesToPending();
        EquityGrantRequestRejectsMovesFromClosed();
    }
}
