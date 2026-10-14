namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelSummary;

using Acme.Workforce.Orgdesign.CostModel.ModelSummary;

/// <summary>Checks of model summary code, run by the project's runner.</summary>
public static class LegacyCostModelGroupViewChecks
{
    public static void LegacyCostModelGroupViewShowsTheStatusInCapitals()
    {
        var item = LegacyCostModelGroup.NewLegacyCostModelGroup("code-168", "label-705");
        var view = LegacyCostModelGroupView.ToLegacyCostModelGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyCostModelGroupView.ToLegacyCostModelGroupViews(new List<LegacyCostModelGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyCostModelGroupViewShowsTheStatusInCapitals();
    }
}
