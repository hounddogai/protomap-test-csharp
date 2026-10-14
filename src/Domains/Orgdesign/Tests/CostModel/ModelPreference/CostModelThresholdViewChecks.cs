namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPreference;

using Acme.Workforce.Orgdesign.CostModel.ModelPreference;

/// <summary>Checks of model preference code, run by the project's runner.</summary>
public static class CostModelThresholdViewChecks
{
    public static void CostModelThresholdViewShowsTheStatusInCapitals()
    {
        var item = CostModelThreshold.NewCostModelThreshold("code-149", "name-254");
        var view = CostModelThresholdView.ToCostModelThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CostModelThresholdView.ToCostModelThresholdViews(new List<CostModelThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CostModelThresholdViewShowsTheStatusInCapitals();
    }
}
