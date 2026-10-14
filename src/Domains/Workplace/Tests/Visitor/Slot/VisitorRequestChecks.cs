namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorRequestChecks
{
    public static void VisitorRequestStartsInDraft()
    {
        var item = VisitorRequest.NewVisitorRequest("code-705", "label-742");
        Check.Equal(item.Status, VisitorRequestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorRequestMovesToActive()
    {
        var item = VisitorRequest.NewVisitorRequest("code-705", "label-742");
        try
        {
            item.MoveTo(VisitorRequestStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorRequestStatus.Active, "status");
    }

    public static void VisitorRequestRejectsMovesFromClosed()
    {
        var item = VisitorRequest.NewVisitorRequest("code-705", "label-742");
        item.Status = VisitorRequestStatus.Closed;
        Check.Fails(() => item.MoveTo(VisitorRequestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        VisitorRequestStartsInDraft();
        VisitorRequestMovesToActive();
        VisitorRequestRejectsMovesFromClosed();
    }
}
