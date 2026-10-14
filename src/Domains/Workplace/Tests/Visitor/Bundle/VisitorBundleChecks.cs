namespace Acme.Workforce.Workplace.Tests.Visitor.Bundle;

using Acme.Workforce.Workplace.Visitor.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class VisitorBundleChecks
{
    public static void VisitorBundleStartsInRequested()
    {
        var item = VisitorBundle.NewVisitorBundle("code-671", "name-590");
        Check.Equal(item.Status, VisitorBundleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorBundleMovesToGranted()
    {
        var item = VisitorBundle.NewVisitorBundle("code-671", "name-590");
        try
        {
            item.MoveTo(VisitorBundleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorBundleStatus.Granted, "status");
    }

    public static void VisitorBundleRejectsMovesFromExpired()
    {
        var item = VisitorBundle.NewVisitorBundle("code-671", "name-590");
        item.Status = VisitorBundleStatus.Expired;
        Check.Fails(() => item.MoveTo(VisitorBundleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        VisitorBundleStartsInRequested();
        VisitorBundleMovesToGranted();
        VisitorBundleRejectsMovesFromExpired();
    }
}
