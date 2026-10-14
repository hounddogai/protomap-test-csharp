namespace Acme.Workforce.Assets.Tests.Warranty.Bundle;

using Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LenientWarrantyBundleChecks
{
    public static void LenientWarrantyBundleStartsInDraft()
    {
        var item = LenientWarrantyBundle.NewLenientWarrantyBundle("code-551", "label-666");
        Check.Equal(item.Status, LenientWarrantyBundleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientWarrantyBundleMovesToPending()
    {
        var item = LenientWarrantyBundle.NewLenientWarrantyBundle("code-551", "label-666");
        try
        {
            item.MoveTo(LenientWarrantyBundleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientWarrantyBundleStatus.Pending, "status");
    }

    public static void LenientWarrantyBundleRejectsMovesFromClosed()
    {
        var item = LenientWarrantyBundle.NewLenientWarrantyBundle("code-551", "label-666");
        item.Status = LenientWarrantyBundleStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientWarrantyBundleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientWarrantyBundleStartsInDraft();
        LenientWarrantyBundleMovesToPending();
        LenientWarrantyBundleRejectsMovesFromClosed();
    }
}
