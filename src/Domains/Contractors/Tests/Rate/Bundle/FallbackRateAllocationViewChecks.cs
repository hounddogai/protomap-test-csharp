namespace Acme.Workforce.Contractors.Tests.Rate.Bundle;

using Acme.Workforce.Contractors.Rate.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackRateAllocationViewChecks
{
    public static void FallbackRateAllocationViewShowsTheStatusInCapitals()
    {
        var item = FallbackRateAllocation.NewFallbackRateAllocation("code-411", "name-285");
        var view = FallbackRateAllocationView.ToFallbackRateAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackRateAllocationView.ToFallbackRateAllocationViews(new List<FallbackRateAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackRateAllocationViewShowsTheStatusInCapitals();
    }
}
