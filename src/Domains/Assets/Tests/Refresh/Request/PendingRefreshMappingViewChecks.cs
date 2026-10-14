namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PendingRefreshMappingViewChecks
{
    public static void PendingRefreshMappingViewShowsTheStatusInCapitals()
    {
        var item = PendingRefreshMapping.NewPendingRefreshMapping("code-902", "label-311");
        var view = PendingRefreshMappingView.ToPendingRefreshMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingRefreshMappingView.ToPendingRefreshMappingViews(new List<PendingRefreshMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingRefreshMappingViewShowsTheStatusInCapitals();
    }
}
