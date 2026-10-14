namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantAssignment;

using Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>Checks of grant assignment code, run by the project's runner.</summary>
public static class GlobalEquityGrantAssignmentChecks
{
    public static void GlobalEquityGrantAssignmentStartsInCreated()
    {
        var item = GlobalEquityGrantAssignment.NewGlobalEquityGrantAssignment("code-875", "label-685");
        Check.Equal(item.Status, GlobalEquityGrantAssignmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalEquityGrantAssignmentMovesToPublished()
    {
        var item = GlobalEquityGrantAssignment.NewGlobalEquityGrantAssignment("code-875", "label-685");
        try
        {
            item.MoveTo(GlobalEquityGrantAssignmentStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalEquityGrantAssignmentStatus.Published, "status");
    }

    public static void GlobalEquityGrantAssignmentRejectsMovesFromRetired()
    {
        var item = GlobalEquityGrantAssignment.NewGlobalEquityGrantAssignment("code-875", "label-685");
        item.Status = GlobalEquityGrantAssignmentStatus.Retired;
        Check.Fails(() => item.MoveTo(GlobalEquityGrantAssignmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        GlobalEquityGrantAssignmentStartsInCreated();
        GlobalEquityGrantAssignmentMovesToPublished();
        GlobalEquityGrantAssignmentRejectsMovesFromRetired();
    }
}
