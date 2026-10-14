namespace Acme.Workforce.Contractors.Tests.Offboarding.Threshold;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class OffboardingAdjustmentViewChecks
{
    public static void OffboardingAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = OffboardingAdjustment.NewOffboardingAdjustment("code-197", "name-943");
        var view = OffboardingAdjustmentView.ToOffboardingAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OffboardingAdjustmentView.ToOffboardingAdjustmentViews(new List<OffboardingAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OffboardingAdjustmentViewShowsTheStatusInCapitals();
    }
}
