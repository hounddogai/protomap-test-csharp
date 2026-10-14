namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class TieredExceptionGroupChecks
{
    public static void TieredExceptionGroupStartsInPlanned()
    {
        var item = TieredExceptionGroup.NewTieredExceptionGroup("code-451", "label-576");
        Check.Equal(item.Status, TieredExceptionGroupStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredExceptionGroupMovesToCompleted()
    {
        var item = TieredExceptionGroup.NewTieredExceptionGroup("code-451", "label-576");
        try
        {
            item.MoveTo(TieredExceptionGroupStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredExceptionGroupStatus.Completed, "status");
    }

    public static void TieredExceptionGroupRejectsMovesFromCancelled()
    {
        var item = TieredExceptionGroup.NewTieredExceptionGroup("code-451", "label-576");
        item.Status = TieredExceptionGroupStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredExceptionGroupStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredExceptionGroupStartsInPlanned();
        TieredExceptionGroupMovesToCompleted();
        TieredExceptionGroupRejectsMovesFromCancelled();
    }
}
