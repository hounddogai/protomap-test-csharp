namespace Acme.Workforce.Workplace.Tests.Visitor.Group;

using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class VisitorGroupChecks
{
    public static void VisitorGroupStartsInProposed()
    {
        var item = VisitorGroup.NewVisitorGroup("code-644", "title-782");
        Check.Equal(item.Status, VisitorGroupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorGroupMovesToWithdrawn()
    {
        var item = VisitorGroup.NewVisitorGroup("code-644", "title-782");
        try
        {
            item.MoveTo(VisitorGroupStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorGroupStatus.Withdrawn, "status");
    }

    public static void VisitorGroupRejectsMovesFromWithdrawn()
    {
        var item = VisitorGroup.NewVisitorGroup("code-644", "title-782");
        item.Status = VisitorGroupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(VisitorGroupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        VisitorGroupStartsInProposed();
        VisitorGroupMovesToWithdrawn();
        VisitorGroupRejectsMovesFromWithdrawn();
    }
}
