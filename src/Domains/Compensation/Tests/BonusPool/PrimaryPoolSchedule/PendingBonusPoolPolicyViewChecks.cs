namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class PendingBonusPoolPolicyViewChecks
{
    public static void PendingBonusPoolPolicyViewShowsTheStatusInCapitals()
    {
        var item = PendingBonusPoolPolicy.NewPendingBonusPoolPolicy("code-601", "title-236");
        var view = PendingBonusPoolPolicyView.ToPendingBonusPoolPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingBonusPoolPolicyView.ToPendingBonusPoolPolicyViews(new List<PendingBonusPoolPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingBonusPoolPolicyViewShowsTheStatusInCapitals();
    }
}
