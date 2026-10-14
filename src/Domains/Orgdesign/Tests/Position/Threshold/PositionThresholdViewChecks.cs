namespace Acme.Workforce.Orgdesign.Tests.Position.Threshold;

using Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PositionThresholdViewChecks
{
    public static void PositionThresholdViewShowsTheStatusInCapitals()
    {
        var item = PositionThreshold.NewPositionThreshold("code-608", "label-114");
        var view = PositionThresholdView.ToPositionThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionThresholdView.ToPositionThresholdViews(new List<PositionThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionThresholdViewShowsTheStatusInCapitals();
    }
}
