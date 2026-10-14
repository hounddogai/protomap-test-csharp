namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class FallbackRefreshPolicyViewChecks
{
    public static void FallbackRefreshPolicyViewShowsTheStatusInCapitals()
    {
        var item = FallbackRefreshPolicy.NewFallbackRefreshPolicy("code-172", "label-690");
        var view = FallbackRefreshPolicyView.ToFallbackRefreshPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackRefreshPolicyView.ToFallbackRefreshPolicyViews(new List<FallbackRefreshPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackRefreshPolicyViewShowsTheStatusInCapitals();
    }
}
