namespace Acme.Workforce.Contractors.Tests.Vendor.Batch;

using Acme.Workforce.Contractors.Vendor.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class VendorExceptionChecks
{
    public static void VendorExceptionStartsInNew()
    {
        var item = VendorException.NewVendorException("code-774", "label-20");
        Check.Equal(item.Status, VendorExceptionStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorExceptionMovesToQueued()
    {
        var item = VendorException.NewVendorException("code-774", "label-20");
        try
        {
            item.MoveTo(VendorExceptionStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorExceptionStatus.Queued, "status");
    }

    public static void VendorExceptionRejectsMovesFromFailed()
    {
        var item = VendorException.NewVendorException("code-774", "label-20");
        item.Status = VendorExceptionStatus.Failed;
        Check.Fails(() => item.MoveTo(VendorExceptionStatus.New), "move");
    }

    public static void RunAll()
    {
        VendorExceptionStartsInNew();
        VendorExceptionMovesToQueued();
        VendorExceptionRejectsMovesFromFailed();
    }
}
