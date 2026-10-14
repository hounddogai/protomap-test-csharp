namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedVisitorAssignmentChecks
{
    public static void CappedVisitorAssignmentStartsInNew()
    {
        var item = CappedVisitorAssignment.NewCappedVisitorAssignment("code-72", "title-204");
        Check.Equal(item.Status, CappedVisitorAssignmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedVisitorAssignmentMovesToFailed()
    {
        var item = CappedVisitorAssignment.NewCappedVisitorAssignment("code-72", "title-204");
        try
        {
            item.MoveTo(CappedVisitorAssignmentStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedVisitorAssignmentStatus.Failed, "status");
    }

    public static void CappedVisitorAssignmentRejectsMovesFromFailed()
    {
        var item = CappedVisitorAssignment.NewCappedVisitorAssignment("code-72", "title-204");
        item.Status = CappedVisitorAssignmentStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedVisitorAssignmentStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedVisitorAssignmentStartsInNew();
        CappedVisitorAssignmentMovesToFailed();
        CappedVisitorAssignmentRejectsMovesFromFailed();
    }
}
