namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingRefreshClaimViewChecks
{
    public static void PendingRefreshClaimViewShowsTheStatusInCapitals()
    {
        var item = PendingRefreshClaim.NewPendingRefreshClaim("code-247", "title-488");
        var view = PendingRefreshClaimView.ToPendingRefreshClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingRefreshClaimView.ToPendingRefreshClaimViews(new List<PendingRefreshClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingRefreshClaimViewShowsTheStatusInCapitals();
    }
}
