namespace Acme.Workforce.Assets.Tests.Refresh.RefreshRequest;

using Acme.Workforce.Assets.Refresh.RefreshRequest;

/// <summary>Checks of refresh request code, run by the project's runner.</summary>
public static class LenientRefreshRequestViewChecks
{
    public static void LenientRefreshRequestViewShowsTheStatusInCapitals()
    {
        var item = LenientRefreshRequest.NewLenientRefreshRequest("code-174", "title-23");
        var view = LenientRefreshRequestView.ToLenientRefreshRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientRefreshRequestView.ToLenientRefreshRequestViews(new List<LenientRefreshRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientRefreshRequestViewShowsTheStatusInCapitals();
    }
}
