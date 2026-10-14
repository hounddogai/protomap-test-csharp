namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class FallbackBonusPoolRequestViewChecks
{
    public static void FallbackBonusPoolRequestViewShowsTheStatusInCapitals()
    {
        var item = FallbackBonusPoolRequest.NewFallbackBonusPoolRequest("code-402", "name-545");
        var view = FallbackBonusPoolRequestView.ToFallbackBonusPoolRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackBonusPoolRequestView.ToFallbackBonusPoolRequestViews(new List<FallbackBonusPoolRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackBonusPoolRequestViewShowsTheStatusInCapitals();
    }
}
