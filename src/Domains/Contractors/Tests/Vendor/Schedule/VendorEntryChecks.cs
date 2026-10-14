namespace Acme.Workforce.Contractors.Tests.Vendor.Schedule;

using Acme.Workforce.Contractors.Vendor.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class VendorEntryChecks
{
    public static void VendorEntryStartsInCreated()
    {
        var item = VendorEntry.NewVendorEntry("code-70", "title-43");
        Check.Equal(item.Status, VendorEntryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorEntryMovesToValidated()
    {
        var item = VendorEntry.NewVendorEntry("code-70", "title-43");
        try
        {
            item.MoveTo(VendorEntryStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorEntryStatus.Validated, "status");
    }

    public static void VendorEntryRejectsMovesFromRetired()
    {
        var item = VendorEntry.NewVendorEntry("code-70", "title-43");
        item.Status = VendorEntryStatus.Retired;
        Check.Fails(() => item.MoveTo(VendorEntryStatus.Created), "move");
    }

    public static void RunAll()
    {
        VendorEntryStartsInCreated();
        VendorEntryMovesToValidated();
        VendorEntryRejectsMovesFromRetired();
    }
}
