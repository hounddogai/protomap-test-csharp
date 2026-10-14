namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdjustmentMappingViewChecks
{
    public static void AdjustmentMappingViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentMapping.NewAdjustmentMapping("code-836", "title-188");
        var view = AdjustmentMappingView.ToAdjustmentMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentMappingView.ToAdjustmentMappingViews(new List<AdjustmentMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentMappingViewShowsTheStatusInCapitals();
    }
}
