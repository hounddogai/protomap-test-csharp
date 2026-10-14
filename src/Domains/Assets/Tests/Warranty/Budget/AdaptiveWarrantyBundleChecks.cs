namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AdaptiveWarrantyBundleChecks
{
    public static void AdaptiveWarrantyBundleStartsInPlanned()
    {
        var item = AdaptiveWarrantyBundle.NewAdaptiveWarrantyBundle("code-54", "name-825");
        Check.Equal(item.Status, AdaptiveWarrantyBundleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveWarrantyBundleMovesToScheduled()
    {
        var item = AdaptiveWarrantyBundle.NewAdaptiveWarrantyBundle("code-54", "name-825");
        try
        {
            item.MoveTo(AdaptiveWarrantyBundleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveWarrantyBundleStatus.Scheduled, "status");
    }

    public static void AdaptiveWarrantyBundleRejectsMovesFromCancelled()
    {
        var item = AdaptiveWarrantyBundle.NewAdaptiveWarrantyBundle("code-54", "name-825");
        item.Status = AdaptiveWarrantyBundleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveWarrantyBundleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyBundleStartsInPlanned();
        AdaptiveWarrantyBundleMovesToScheduled();
        AdaptiveWarrantyBundleRejectsMovesFromCancelled();
    }
}
