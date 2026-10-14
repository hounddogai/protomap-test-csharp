namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class AdjustmentTargetChecks
{
    public static void AdjustmentTargetStartsInOpen()
    {
        var item = AdjustmentTarget.NewAdjustmentTarget("code-12", "label-137");
        Check.Equal(item.Status, AdjustmentTargetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentTargetMovesToInReview()
    {
        var item = AdjustmentTarget.NewAdjustmentTarget("code-12", "label-137");
        try
        {
            item.MoveTo(AdjustmentTargetStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentTargetStatus.InReview, "status");
    }

    public static void AdjustmentTargetRejectsMovesFromArchived()
    {
        var item = AdjustmentTarget.NewAdjustmentTarget("code-12", "label-137");
        item.Status = AdjustmentTargetStatus.Archived;
        Check.Fails(() => item.MoveTo(AdjustmentTargetStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdjustmentTargetStartsInOpen();
        AdjustmentTargetMovesToInReview();
        AdjustmentTargetRejectsMovesFromArchived();
    }
}
