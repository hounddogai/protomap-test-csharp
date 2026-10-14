namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorOverride;

using Acme.Workforce.Workplace.Visitor.VisitorOverride;

/// <summary>Checks of visitor override code, run by the project's runner.</summary>
public static class PrimaryVisitorOverrideChecks
{
    public static void PrimaryVisitorOverrideStartsInDraft()
    {
        var item = PrimaryVisitorOverride.NewPrimaryVisitorOverride("code-353", "name-807");
        Check.Equal(item.Status, PrimaryVisitorOverrideStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryVisitorOverrideMovesToPending()
    {
        var item = PrimaryVisitorOverride.NewPrimaryVisitorOverride("code-353", "name-807");
        try
        {
            item.MoveTo(PrimaryVisitorOverrideStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryVisitorOverrideStatus.Pending, "status");
    }

    public static void PrimaryVisitorOverrideRejectsMovesFromClosed()
    {
        var item = PrimaryVisitorOverride.NewPrimaryVisitorOverride("code-353", "name-807");
        item.Status = PrimaryVisitorOverrideStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryVisitorOverrideStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryVisitorOverrideStartsInDraft();
        PrimaryVisitorOverrideMovesToPending();
        PrimaryVisitorOverrideRejectsMovesFromClosed();
    }
}
