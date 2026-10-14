namespace Acme.Workforce.Assets.Tests.Warranty.Summary;

using Acme.Workforce.Assets.Warranty.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WarrantyBundleChecks
{
    public static void WarrantyBundleStartsInRequested()
    {
        var item = WarrantyBundle.NewWarrantyBundle("code-798", "title-380");
        Check.Equal(item.Status, WarrantyBundleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyBundleMovesToGranted()
    {
        var item = WarrantyBundle.NewWarrantyBundle("code-798", "title-380");
        try
        {
            item.MoveTo(WarrantyBundleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyBundleStatus.Granted, "status");
    }

    public static void WarrantyBundleRejectsMovesFromExpired()
    {
        var item = WarrantyBundle.NewWarrantyBundle("code-798", "title-380");
        item.Status = WarrantyBundleStatus.Expired;
        Check.Fails(() => item.MoveTo(WarrantyBundleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        WarrantyBundleStartsInRequested();
        WarrantyBundleMovesToGranted();
        WarrantyBundleRejectsMovesFromExpired();
    }
}
