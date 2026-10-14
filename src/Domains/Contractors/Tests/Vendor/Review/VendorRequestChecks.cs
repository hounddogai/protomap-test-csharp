namespace Acme.Workforce.Contractors.Tests.Vendor.Review;

using Acme.Workforce.Contractors.Vendor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VendorRequestChecks
{
    public static void VendorRequestStartsInOpen()
    {
        var item = VendorRequest.NewVendorRequest("code-556", "label-912");
        Check.Equal(item.Status, VendorRequestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorRequestMovesToArchived()
    {
        var item = VendorRequest.NewVendorRequest("code-556", "label-912");
        try
        {
            item.MoveTo(VendorRequestStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorRequestStatus.Archived, "status");
    }

    public static void VendorRequestRejectsMovesFromArchived()
    {
        var item = VendorRequest.NewVendorRequest("code-556", "label-912");
        item.Status = VendorRequestStatus.Archived;
        Check.Fails(() => item.MoveTo(VendorRequestStatus.Open), "move");
    }

    public static void RunAll()
    {
        VendorRequestStartsInOpen();
        VendorRequestMovesToArchived();
        VendorRequestRejectsMovesFromArchived();
    }
}
