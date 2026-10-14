namespace Acme.Workforce.Contractors.Tests.Vendor.Batch;

using Acme.Workforce.Contractors.Vendor.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class VendorExceptionViewChecks
{
    public static void VendorExceptionViewShowsTheStatusInCapitals()
    {
        var item = VendorException.NewVendorException("code-774", "label-20");
        var view = VendorExceptionView.ToVendorExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorExceptionView.ToVendorExceptionViews(new List<VendorException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorExceptionViewShowsTheStatusInCapitals();
    }
}
