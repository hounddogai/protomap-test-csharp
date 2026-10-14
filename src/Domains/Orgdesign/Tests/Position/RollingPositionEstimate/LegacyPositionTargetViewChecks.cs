namespace Acme.Workforce.Orgdesign.Tests.Position.RollingPositionEstimate;

using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Checks of rolling position estimate code, run by the project's runner.</summary>
public static class LegacyPositionTargetViewChecks
{
    public static void LegacyPositionTargetViewShowsTheStatusInCapitals()
    {
        var item = LegacyPositionTarget.NewLegacyPositionTarget("code-666", "title-561");
        var view = LegacyPositionTargetView.ToLegacyPositionTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPositionTargetView.ToLegacyPositionTargetViews(new List<LegacyPositionTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPositionTargetViewShowsTheStatusInCapitals();
    }
}
