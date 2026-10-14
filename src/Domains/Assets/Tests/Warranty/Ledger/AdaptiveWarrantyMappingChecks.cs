namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveWarrantyMappingChecks
{
    public static void AdaptiveWarrantyMappingStartsInDraft()
    {
        var item = AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping("code-248", "label-211");
        Check.Equal(item.Status, AdaptiveWarrantyMappingStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveWarrantyMappingMovesToPending()
    {
        var item = AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping("code-248", "label-211");
        try
        {
            item.MoveTo(AdaptiveWarrantyMappingStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveWarrantyMappingStatus.Pending, "status");
    }

    public static void AdaptiveWarrantyMappingRejectsMovesFromClosed()
    {
        var item = AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping("code-248", "label-211");
        item.Status = AdaptiveWarrantyMappingStatus.Closed;
        Check.Fails(() => item.MoveTo(AdaptiveWarrantyMappingStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyMappingStartsInDraft();
        AdaptiveWarrantyMappingMovesToPending();
        AdaptiveWarrantyMappingRejectsMovesFromClosed();
    }
}
