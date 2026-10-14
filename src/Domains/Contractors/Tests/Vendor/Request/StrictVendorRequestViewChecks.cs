namespace Acme.Workforce.Contractors.Tests.Vendor.Request;

using Acme.Workforce.Contractors.Vendor.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class StrictVendorRequestViewChecks
{
    public static void StrictVendorRequestViewShowsTheStatusInCapitals()
    {
        var item = StrictVendorRequest.NewStrictVendorRequest("code-706", "title-187");
        var view = StrictVendorRequestView.ToStrictVendorRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictVendorRequestView.ToStrictVendorRequestViews(new List<StrictVendorRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictVendorRequestViewShowsTheStatusInCapitals();
    }
}
