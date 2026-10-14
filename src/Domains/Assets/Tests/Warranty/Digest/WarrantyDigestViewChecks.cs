namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class WarrantyDigestViewChecks
{
    public static void WarrantyDigestViewShowsTheStatusInCapitals()
    {
        var item = WarrantyDigest.NewWarrantyDigest("code-70", "label-447");
        var view = WarrantyDigestView.ToWarrantyDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyDigestView.ToWarrantyDigestViews(new List<WarrantyDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyDigestViewShowsTheStatusInCapitals();
    }
}
