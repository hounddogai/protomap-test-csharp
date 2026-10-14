namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class VendorLedgerChecks
{
    public static void VendorLedgerStartsInCreated()
    {
        var item = VendorLedger.NewVendorLedger("code-639", "title-65");
        Check.Equal(item.Status, VendorLedgerStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorLedgerMovesToValidated()
    {
        var item = VendorLedger.NewVendorLedger("code-639", "title-65");
        try
        {
            item.MoveTo(VendorLedgerStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorLedgerStatus.Validated, "status");
    }

    public static void VendorLedgerRejectsMovesFromRetired()
    {
        var item = VendorLedger.NewVendorLedger("code-639", "title-65");
        item.Status = VendorLedgerStatus.Retired;
        Check.Fails(() => item.MoveTo(VendorLedgerStatus.Created), "move");
    }

    public static void RunAll()
    {
        VendorLedgerStartsInCreated();
        VendorLedgerMovesToValidated();
        VendorLedgerRejectsMovesFromRetired();
    }
}
