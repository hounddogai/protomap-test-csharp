namespace Acme.Workforce.Contractors.Tests.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

/// <summary>Checks of capped offboarding slot code, run by the project's runner.</summary>
public static class BatchedOffboardingAllocationViewChecks
{
    public static void BatchedOffboardingAllocationViewShowsTheStatusInCapitals()
    {
        var item = BatchedOffboardingAllocation.NewBatchedOffboardingAllocation("code-632", "label-224");
        var view = BatchedOffboardingAllocationView.ToBatchedOffboardingAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedOffboardingAllocationView.ToBatchedOffboardingAllocationViews(new List<BatchedOffboardingAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedOffboardingAllocationViewShowsTheStatusInCapitals();
    }
}
