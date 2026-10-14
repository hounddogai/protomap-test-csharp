namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class LenientBonusPoolPolicyViewChecks
{
    public static void LenientBonusPoolPolicyViewShowsTheStatusInCapitals()
    {
        var item = LenientBonusPoolPolicy.NewLenientBonusPoolPolicy("code-372", "name-349");
        var view = LenientBonusPoolPolicyView.ToLenientBonusPoolPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientBonusPoolPolicyView.ToLenientBonusPoolPolicyViews(new List<LenientBonusPoolPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientBonusPoolPolicyViewShowsTheStatusInCapitals();
    }
}
