namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CostModelAllocationViewChecks
{
    public static void CostModelAllocationViewShowsTheStatusInCapitals()
    {
        var item = CostModelAllocation.NewCostModelAllocation("code-548", "label-346");
        var view = CostModelAllocationView.ToCostModelAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CostModelAllocationView.ToCostModelAllocationViews(new List<CostModelAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CostModelAllocationViewShowsTheStatusInCapitals();
    }
}
