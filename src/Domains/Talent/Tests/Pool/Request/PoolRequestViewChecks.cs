namespace Acme.Workforce.Talent.Tests.Pool.Request;

using Acme.Workforce.Talent.Pool.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PoolRequestViewChecks
{
    public static void PoolRequestViewShowsTheStatusInCapitals()
    {
        var item = PoolRequest.NewPoolRequest("code-673", "name-19");
        var view = PoolRequestView.ToPoolRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolRequestView.ToPoolRequestViews(new List<PoolRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolRequestViewShowsTheStatusInCapitals();
    }
}
