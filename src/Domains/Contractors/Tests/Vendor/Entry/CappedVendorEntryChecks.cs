namespace Acme.Workforce.Contractors.Tests.Vendor.Entry;

using Acme.Workforce.Contractors.Vendor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CappedVendorEntryChecks
{
    public static void CappedVendorEntryStartsInDraft()
    {
        var item = CappedVendorEntry.NewCappedVendorEntry("code-497", "name-640");
        Check.Equal(item.Status, CappedVendorEntryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedVendorEntryMovesToActive()
    {
        var item = CappedVendorEntry.NewCappedVendorEntry("code-497", "name-640");
        try
        {
            item.MoveTo(CappedVendorEntryStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedVendorEntryStatus.Active, "status");
    }

    public static void CappedVendorEntryRejectsMovesFromClosed()
    {
        var item = CappedVendorEntry.NewCappedVendorEntry("code-497", "name-640");
        item.Status = CappedVendorEntryStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedVendorEntryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedVendorEntryStartsInDraft();
        CappedVendorEntryMovesToActive();
        CappedVendorEntryRejectsMovesFromClosed();
    }
}
