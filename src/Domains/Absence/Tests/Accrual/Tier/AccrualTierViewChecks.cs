namespace Acme.Workforce.Absence.Tests.Accrual.Tier;

using Acme.Workforce.Absence.Accrual.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccrualTierViewChecks
{
    public static void AccrualTierViewShowsTheStatusInCapitals()
    {
        var item = AccrualTier.NewAccrualTier("code-140", "label-970");
        var view = AccrualTierView.ToAccrualTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualTierView.ToAccrualTierViews(new List<AccrualTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualTierViewShowsTheStatusInCapitals();
    }
}
