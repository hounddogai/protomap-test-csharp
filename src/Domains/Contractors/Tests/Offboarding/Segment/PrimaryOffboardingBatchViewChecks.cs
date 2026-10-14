namespace Acme.Workforce.Contractors.Tests.Offboarding.Segment;

using Acme.Workforce.Contractors.Offboarding.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PrimaryOffboardingBatchViewChecks
{
    public static void PrimaryOffboardingBatchViewShowsTheStatusInCapitals()
    {
        var item = PrimaryOffboardingBatch.NewPrimaryOffboardingBatch("code-824", "name-368");
        var view = PrimaryOffboardingBatchView.ToPrimaryOffboardingBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryOffboardingBatchView.ToPrimaryOffboardingBatchViews(new List<PrimaryOffboardingBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryOffboardingBatchViewShowsTheStatusInCapitals();
    }
}
