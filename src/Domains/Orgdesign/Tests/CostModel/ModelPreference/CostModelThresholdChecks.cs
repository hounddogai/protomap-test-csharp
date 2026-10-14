namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPreference;

using Acme.Workforce.Orgdesign.CostModel.ModelPreference;

/// <summary>Checks of model preference code, run by the project's runner.</summary>
public static class CostModelThresholdChecks
{
    public static void CostModelThresholdStartsInPlanned()
    {
        var item = CostModelThreshold.NewCostModelThreshold("code-149", "name-254");
        Check.Equal(item.Status, CostModelThresholdStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelThresholdMovesToCancelled()
    {
        var item = CostModelThreshold.NewCostModelThreshold("code-149", "name-254");
        try
        {
            item.MoveTo(CostModelThresholdStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelThresholdStatus.Cancelled, "status");
    }

    public static void CostModelThresholdRejectsMovesFromCancelled()
    {
        var item = CostModelThreshold.NewCostModelThreshold("code-149", "name-254");
        item.Status = CostModelThresholdStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CostModelThresholdStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CostModelThresholdStartsInPlanned();
        CostModelThresholdMovesToCancelled();
        CostModelThresholdRejectsMovesFromCancelled();
    }
}
