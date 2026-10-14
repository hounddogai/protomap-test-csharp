namespace Acme.Workforce.Contractors.Tests.Vendor.Assignment;

using Acme.Workforce.Contractors.Vendor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VendorPreferenceChecks
{
    public static void VendorPreferenceStartsInNew()
    {
        var item = VendorPreference.NewVendorPreference("code-553", "label-302");
        Check.Equal(item.Status, VendorPreferenceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorPreferenceMovesToDone()
    {
        var item = VendorPreference.NewVendorPreference("code-553", "label-302");
        try
        {
            item.MoveTo(VendorPreferenceStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorPreferenceStatus.Done, "status");
    }

    public static void VendorPreferenceRejectsMovesFromFailed()
    {
        var item = VendorPreference.NewVendorPreference("code-553", "label-302");
        item.Status = VendorPreferenceStatus.Failed;
        Check.Fails(() => item.MoveTo(VendorPreferenceStatus.New), "move");
    }

    public static void RunAll()
    {
        VendorPreferenceStartsInNew();
        VendorPreferenceMovesToDone();
        VendorPreferenceRejectsMovesFromFailed();
    }
}
