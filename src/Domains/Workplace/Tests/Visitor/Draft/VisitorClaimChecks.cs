namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class VisitorClaimChecks
{
    public static void VisitorClaimStartsInDraft()
    {
        var item = VisitorClaim.NewVisitorClaim("code-857", "label-600");
        Check.Equal(item.Status, VisitorClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorClaimMovesToActive()
    {
        var item = VisitorClaim.NewVisitorClaim("code-857", "label-600");
        try
        {
            item.MoveTo(VisitorClaimStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorClaimStatus.Active, "status");
    }

    public static void VisitorClaimRejectsMovesFromClosed()
    {
        var item = VisitorClaim.NewVisitorClaim("code-857", "label-600");
        item.Status = VisitorClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(VisitorClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        VisitorClaimStartsInDraft();
        VisitorClaimMovesToActive();
        VisitorClaimRejectsMovesFromClosed();
    }
}
