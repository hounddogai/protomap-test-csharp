namespace Acme.Workforce.Orgdesign.Tests.Scenario.Policy;

using Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TieredScenarioDraftChecks
{
    public static void TieredScenarioDraftStartsInPlanned()
    {
        var item = TieredScenarioDraft.NewTieredScenarioDraft("code-290", "label-834");
        Check.Equal(item.Status, TieredScenarioDraftStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredScenarioDraftMovesToCompleted()
    {
        var item = TieredScenarioDraft.NewTieredScenarioDraft("code-290", "label-834");
        try
        {
            item.MoveTo(TieredScenarioDraftStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredScenarioDraftStatus.Completed, "status");
    }

    public static void TieredScenarioDraftRejectsMovesFromCancelled()
    {
        var item = TieredScenarioDraft.NewTieredScenarioDraft("code-290", "label-834");
        item.Status = TieredScenarioDraftStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredScenarioDraftStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredScenarioDraftStartsInPlanned();
        TieredScenarioDraftMovesToCompleted();
        TieredScenarioDraftRejectsMovesFromCancelled();
    }
}
