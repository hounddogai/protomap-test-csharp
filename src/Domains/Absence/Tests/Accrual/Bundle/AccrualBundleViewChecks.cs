namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AccrualBundleViewChecks
{
    public static void AccrualBundleViewShowsTheStatusInCapitals()
    {
        var item = AccrualBundle.NewAccrualBundle("code-858", "name-894");
        var view = AccrualBundleView.ToAccrualBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualBundleView.ToAccrualBundleViews(new List<AccrualBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualBundleViewShowsTheStatusInCapitals();
    }
}
