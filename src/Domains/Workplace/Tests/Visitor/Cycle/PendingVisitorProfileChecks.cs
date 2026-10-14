namespace Acme.Workforce.Workplace.Tests.Visitor.Cycle;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PendingVisitorProfileChecks
{
    public static void PendingVisitorProfileStartsInCreated()
    {
        var item = PendingVisitorProfile.NewPendingVisitorProfile("code-416", "title-123");
        Check.Equal(item.Status, PendingVisitorProfileStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingVisitorProfileMovesToPublished()
    {
        var item = PendingVisitorProfile.NewPendingVisitorProfile("code-416", "title-123");
        try
        {
            item.MoveTo(PendingVisitorProfileStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingVisitorProfileStatus.Published, "status");
    }

    public static void PendingVisitorProfileRejectsMovesFromRetired()
    {
        var item = PendingVisitorProfile.NewPendingVisitorProfile("code-416", "title-123");
        item.Status = PendingVisitorProfileStatus.Retired;
        Check.Fails(() => item.MoveTo(PendingVisitorProfileStatus.Created), "move");
    }

    public static void RunAll()
    {
        PendingVisitorProfileStartsInCreated();
        PendingVisitorProfileMovesToPublished();
        PendingVisitorProfileRejectsMovesFromRetired();
    }
}
