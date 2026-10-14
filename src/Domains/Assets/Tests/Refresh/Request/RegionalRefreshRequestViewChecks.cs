namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RegionalRefreshRequestViewChecks
{
    public static void RegionalRefreshRequestViewShowsTheStatusInCapitals()
    {
        var item = RegionalRefreshRequest.NewRegionalRefreshRequest("code-620", "label-996");
        var view = RegionalRefreshRequestView.ToRegionalRefreshRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalRefreshRequestView.ToRegionalRefreshRequestViews(new List<RegionalRefreshRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalRefreshRequestViewShowsTheStatusInCapitals();
    }
}
