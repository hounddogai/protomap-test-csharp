namespace Acme.Workforce.Contractors.Tests.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class StrictVendorRequestChecks
{
    public static void StrictVendorRequestStartsInProposed()
    {
        var item = StrictVendorRequest.NewStrictVendorRequest("code-706", "title-187");
        Check.Equal(item.Status, StrictVendorRequestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictVendorRequestMovesToWithdrawn()
    {
        var item = StrictVendorRequest.NewStrictVendorRequest("code-706", "title-187");
        try
        {
            item.MoveTo(StrictVendorRequestStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictVendorRequestStatus.Withdrawn, "status");
    }

    public static void StrictVendorRequestRejectsMovesFromWithdrawn()
    {
        var item = StrictVendorRequest.NewStrictVendorRequest("code-706", "title-187");
        item.Status = StrictVendorRequestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictVendorRequestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictVendorRequestStartsInProposed();
        StrictVendorRequestMovesToWithdrawn();
        StrictVendorRequestRejectsMovesFromWithdrawn();
    }
}
