namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class GlobalCostModelGroupChecks
{
    public static void GlobalCostModelGroupStartsInIdle()
    {
        var item = GlobalCostModelGroup.NewGlobalCostModelGroup("code-71", "name-271");
        Check.Equal(item.Status, GlobalCostModelGroupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalCostModelGroupMovesToWarming()
    {
        var item = GlobalCostModelGroup.NewGlobalCostModelGroup("code-71", "name-271");
        try
        {
            item.MoveTo(GlobalCostModelGroupStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalCostModelGroupStatus.Warming, "status");
    }

    public static void GlobalCostModelGroupRejectsMovesFromStopped()
    {
        var item = GlobalCostModelGroup.NewGlobalCostModelGroup("code-71", "name-271");
        item.Status = GlobalCostModelGroupStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalCostModelGroupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalCostModelGroupStartsInIdle();
        GlobalCostModelGroupMovesToWarming();
        GlobalCostModelGroupRejectsMovesFromStopped();
    }
}
