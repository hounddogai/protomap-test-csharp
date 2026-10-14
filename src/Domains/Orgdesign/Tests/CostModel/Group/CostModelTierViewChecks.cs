namespace Acme.Workforce.Orgdesign.Tests.CostModel.Group;

using Acme.Workforce.Orgdesign.CostModel.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class CostModelTierViewChecks
{
    public static void CostModelTierViewShowsTheStatusInCapitals()
    {
        var item = CostModelTier.NewCostModelTier("code-995", "name-304");
        var view = CostModelTierView.ToCostModelTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CostModelTierView.ToCostModelTierViews(new List<CostModelTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CostModelTierViewShowsTheStatusInCapitals();
    }
}
