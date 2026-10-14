namespace Acme.Workforce.Workplace.Tests.Visitor.Baseline;

using Acme.Workforce.Workplace.Visitor.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class VisitorBaselineChecks
{
    public static void VisitorBaselineStartsInRequested()
    {
        var item = VisitorBaseline.NewVisitorBaseline("code-720", "title-31");
        Check.Equal(item.Status, VisitorBaselineStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorBaselineMovesToGranted()
    {
        var item = VisitorBaseline.NewVisitorBaseline("code-720", "title-31");
        try
        {
            item.MoveTo(VisitorBaselineStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorBaselineStatus.Granted, "status");
    }

    public static void VisitorBaselineRejectsMovesFromExpired()
    {
        var item = VisitorBaseline.NewVisitorBaseline("code-720", "title-31");
        item.Status = VisitorBaselineStatus.Expired;
        Check.Fails(() => item.MoveTo(VisitorBaselineStatus.Requested), "move");
    }

    public static void RunAll()
    {
        VisitorBaselineStartsInRequested();
        VisitorBaselineMovesToGranted();
        VisitorBaselineRejectsMovesFromExpired();
    }
}
