namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class VisitorRollupChecks
{
    public static void VisitorRollupStartsInNew()
    {
        var item = VisitorRollup.NewVisitorRollup("code-501", "name-739");
        Check.Equal(item.Status, VisitorRollupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorRollupMovesToDone()
    {
        var item = VisitorRollup.NewVisitorRollup("code-501", "name-739");
        try
        {
            item.MoveTo(VisitorRollupStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorRollupStatus.Done, "status");
    }

    public static void VisitorRollupRejectsMovesFromFailed()
    {
        var item = VisitorRollup.NewVisitorRollup("code-501", "name-739");
        item.Status = VisitorRollupStatus.Failed;
        Check.Fails(() => item.MoveTo(VisitorRollupStatus.New), "move");
    }

    public static void RunAll()
    {
        VisitorRollupStartsInNew();
        VisitorRollupMovesToDone();
        VisitorRollupRejectsMovesFromFailed();
    }
}
