namespace Acme.Workforce.Orgdesign.Tests.Position.RollingPositionEstimate;

using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Checks of rolling position estimate code, run by the project's runner.</summary>
public static class LegacyPositionEstimateViewChecks
{
    public static void LegacyPositionEstimateViewShowsTheStatusInCapitals()
    {
        var item = LegacyPositionEstimate.NewLegacyPositionEstimate("code-588", "label-249");
        var view = LegacyPositionEstimateView.ToLegacyPositionEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPositionEstimateView.ToLegacyPositionEstimateViews(new List<LegacyPositionEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPositionEstimateViewShowsTheStatusInCapitals();
    }
}
