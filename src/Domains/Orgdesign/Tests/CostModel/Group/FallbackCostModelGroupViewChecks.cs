namespace Acme.Workforce.Orgdesign.Tests.CostModel.Group;

using Acme.Workforce.Orgdesign.CostModel.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class FallbackCostModelGroupViewChecks
{
    public static void FallbackCostModelGroupViewShowsTheStatusInCapitals()
    {
        var item = FallbackCostModelGroup.NewFallbackCostModelGroup("code-755", "title-767");
        var view = FallbackCostModelGroupView.ToFallbackCostModelGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackCostModelGroupView.ToFallbackCostModelGroupViews(new List<FallbackCostModelGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackCostModelGroupViewShowsTheStatusInCapitals();
    }
}
