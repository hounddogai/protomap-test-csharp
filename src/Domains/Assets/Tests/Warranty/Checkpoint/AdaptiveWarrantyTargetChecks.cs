namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AdaptiveWarrantyTargetChecks
{
    public static void AdaptiveWarrantyTargetStartsInOpen()
    {
        var item = AdaptiveWarrantyTarget.NewAdaptiveWarrantyTarget("code-750", "title-427");
        Check.Equal(item.Status, AdaptiveWarrantyTargetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveWarrantyTargetMovesToResolved()
    {
        var item = AdaptiveWarrantyTarget.NewAdaptiveWarrantyTarget("code-750", "title-427");
        try
        {
            item.MoveTo(AdaptiveWarrantyTargetStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveWarrantyTargetStatus.Resolved, "status");
    }

    public static void AdaptiveWarrantyTargetRejectsMovesFromArchived()
    {
        var item = AdaptiveWarrantyTarget.NewAdaptiveWarrantyTarget("code-750", "title-427");
        item.Status = AdaptiveWarrantyTargetStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveWarrantyTargetStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyTargetStartsInOpen();
        AdaptiveWarrantyTargetMovesToResolved();
        AdaptiveWarrantyTargetRejectsMovesFromArchived();
    }
}
