namespace Acme.Workforce.Workplace.Tests.Visitor.Summary;

using Acme.Workforce.Workplace.Visitor.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class VisitorWindowChecks
{
    public static void VisitorWindowStartsInRequested()
    {
        var item = VisitorWindow.NewVisitorWindow("code-268", "name-511");
        Check.Equal(item.Status, VisitorWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorWindowMovesToGranted()
    {
        var item = VisitorWindow.NewVisitorWindow("code-268", "name-511");
        try
        {
            item.MoveTo(VisitorWindowStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorWindowStatus.Granted, "status");
    }

    public static void VisitorWindowRejectsMovesFromExpired()
    {
        var item = VisitorWindow.NewVisitorWindow("code-268", "name-511");
        item.Status = VisitorWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(VisitorWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        VisitorWindowStartsInRequested();
        VisitorWindowMovesToGranted();
        VisitorWindowRejectsMovesFromExpired();
    }
}
