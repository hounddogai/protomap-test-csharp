namespace Acme.Workforce.Orgdesign.Tests.Reorg.Claim;

using Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ReorgClaimViewChecks
{
    public static void ReorgClaimViewShowsTheStatusInCapitals()
    {
        var item = ReorgClaim.NewReorgClaim("code-906", "title-918");
        var view = ReorgClaimView.ToReorgClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgClaimView.ToReorgClaimViews(new List<ReorgClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgClaimViewShowsTheStatusInCapitals();
    }
}
