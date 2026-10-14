namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class RollingRecognitionBaselineViewChecks
{
    public static void RollingRecognitionBaselineViewShowsTheStatusInCapitals()
    {
        var item = RollingRecognitionBaseline.NewRollingRecognitionBaseline("code-652", "title-496");
        var view = RollingRecognitionBaselineView.ToRollingRecognitionBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingRecognitionBaselineView.ToRollingRecognitionBaselineViews(new List<RollingRecognitionBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingRecognitionBaselineViewShowsTheStatusInCapitals();
    }
}
