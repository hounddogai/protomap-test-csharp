namespace Acme.Workforce.Workplace.Tests.Visitor.OverrideItem;

using Acme.Workforce.Workplace.Visitor.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class VisitorOverrideChecks
{
    public static void VisitorOverrideStartsInDraft()
    {
        var item = VisitorOverride.NewVisitorOverride("code-24", "name-850");
        Check.Equal(item.Status, VisitorOverrideStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorOverrideMovesToActive()
    {
        var item = VisitorOverride.NewVisitorOverride("code-24", "name-850");
        try
        {
            item.MoveTo(VisitorOverrideStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorOverrideStatus.Active, "status");
    }

    public static void VisitorOverrideRejectsMovesFromClosed()
    {
        var item = VisitorOverride.NewVisitorOverride("code-24", "name-850");
        item.Status = VisitorOverrideStatus.Closed;
        Check.Fails(() => item.MoveTo(VisitorOverrideStatus.Draft), "move");
    }

    public static void RunAll()
    {
        VisitorOverrideStartsInDraft();
        VisitorOverrideMovesToActive();
        VisitorOverrideRejectsMovesFromClosed();
    }
}
