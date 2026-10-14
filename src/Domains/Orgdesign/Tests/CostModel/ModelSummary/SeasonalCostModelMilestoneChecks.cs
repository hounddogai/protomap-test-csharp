namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class SeasonalCostModelMilestoneChecks
{
    public static void SeasonalCostModelMilestoneStartsInDraft()
    {
        var item = SeasonalCostModelMilestone.NewSeasonalCostModelMilestone("code-488", "label-772");
        Check.Equal(item.Status, SeasonalCostModelMilestoneStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalCostModelMilestoneMovesToActive()
    {
        var item = SeasonalCostModelMilestone.NewSeasonalCostModelMilestone("code-488", "label-772");
        try
        {
            item.MoveTo(SeasonalCostModelMilestoneStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalCostModelMilestoneStatus.Active, "status");
    }

    public static void SeasonalCostModelMilestoneRejectsMovesFromClosed()
    {
        var item = SeasonalCostModelMilestone.NewSeasonalCostModelMilestone("code-488", "label-772");
        item.Status = SeasonalCostModelMilestoneStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalCostModelMilestoneStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalCostModelMilestoneStartsInDraft();
        SeasonalCostModelMilestoneMovesToActive();
        SeasonalCostModelMilestoneRejectsMovesFromClosed();
    }
}
