namespace Acme.Workforce.Orgdesign.Tests.Headcount.Allocation;

using Acme.Workforce.Orgdesign.Headcount.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class HeadcountAllocationViewChecks
{
    public static void HeadcountAllocationViewShowsTheStatusInCapitals()
    {
        var item = HeadcountAllocation.NewHeadcountAllocation("code-921", "label-17");
        var view = HeadcountAllocationView.ToHeadcountAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountAllocationView.ToHeadcountAllocationViews(new List<HeadcountAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountAllocationViewShowsTheStatusInCapitals();
    }
}
