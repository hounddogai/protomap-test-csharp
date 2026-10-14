namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class ReorgAdjustmentChecks
{
    public static void ReorgAdjustmentStartsInOpen()
    {
        var item = ReorgAdjustment.NewReorgAdjustment("code-810", "label-575");
        Check.Equal(item.Status, ReorgAdjustmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgAdjustmentMovesToInReview()
    {
        var item = ReorgAdjustment.NewReorgAdjustment("code-810", "label-575");
        try
        {
            item.MoveTo(ReorgAdjustmentStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgAdjustmentStatus.InReview, "status");
    }

    public static void ReorgAdjustmentRejectsMovesFromArchived()
    {
        var item = ReorgAdjustment.NewReorgAdjustment("code-810", "label-575");
        item.Status = ReorgAdjustmentStatus.Archived;
        Check.Fails(() => item.MoveTo(ReorgAdjustmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        ReorgAdjustmentStartsInOpen();
        ReorgAdjustmentMovesToInReview();
        ReorgAdjustmentRejectsMovesFromArchived();
    }
}
