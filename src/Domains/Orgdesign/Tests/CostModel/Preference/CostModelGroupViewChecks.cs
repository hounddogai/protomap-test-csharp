namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class CostModelGroupViewChecks
{
    public static void CostModelGroupViewShowsTheStatusInCapitals()
    {
        var item = CostModelGroup.NewCostModelGroup("code-404", "name-897");
        var view = CostModelGroupView.ToCostModelGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CostModelGroupView.ToCostModelGroupViews(new List<CostModelGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CostModelGroupViewShowsTheStatusInCapitals();
    }
}
