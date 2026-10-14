namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolRequestViewChecks
{
    public static void BonusPoolRequestViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolRequest.NewBonusPoolRequest("code-557", "title-986");
        var view = BonusPoolRequestView.ToBonusPoolRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolRequestView.ToBonusPoolRequestViews(new List<BonusPoolRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolRequestViewShowsTheStatusInCapitals();
    }
}
