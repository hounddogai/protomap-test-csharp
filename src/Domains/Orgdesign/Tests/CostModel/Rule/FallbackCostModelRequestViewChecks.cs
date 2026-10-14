namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FallbackCostModelRequestViewChecks
{
    public static void FallbackCostModelRequestViewShowsTheStatusInCapitals()
    {
        var item = FallbackCostModelRequest.NewFallbackCostModelRequest("code-145", "label-743");
        var view = FallbackCostModelRequestView.ToFallbackCostModelRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackCostModelRequestView.ToFallbackCostModelRequestViews(new List<FallbackCostModelRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackCostModelRequestViewShowsTheStatusInCapitals();
    }
}
