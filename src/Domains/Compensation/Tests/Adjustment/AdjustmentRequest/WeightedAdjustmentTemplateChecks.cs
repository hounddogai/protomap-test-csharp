namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class WeightedAdjustmentTemplateChecks
{
    public static void WeightedAdjustmentTemplateStartsInOpen()
    {
        var item = WeightedAdjustmentTemplate.NewWeightedAdjustmentTemplate("code-4", "label-838");
        Check.Equal(item.Status, WeightedAdjustmentTemplateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAdjustmentTemplateMovesToInReview()
    {
        var item = WeightedAdjustmentTemplate.NewWeightedAdjustmentTemplate("code-4", "label-838");
        try
        {
            item.MoveTo(WeightedAdjustmentTemplateStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAdjustmentTemplateStatus.InReview, "status");
    }

    public static void WeightedAdjustmentTemplateRejectsMovesFromArchived()
    {
        var item = WeightedAdjustmentTemplate.NewWeightedAdjustmentTemplate("code-4", "label-838");
        item.Status = WeightedAdjustmentTemplateStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedAdjustmentTemplateStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedAdjustmentTemplateStartsInOpen();
        WeightedAdjustmentTemplateMovesToInReview();
        WeightedAdjustmentTemplateRejectsMovesFromArchived();
    }
}
