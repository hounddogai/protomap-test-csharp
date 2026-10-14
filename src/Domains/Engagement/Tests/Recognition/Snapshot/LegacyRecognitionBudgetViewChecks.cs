namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyRecognitionBudgetViewChecks
{
    public static void LegacyRecognitionBudgetViewShowsTheStatusInCapitals()
    {
        var item = LegacyRecognitionBudget.NewLegacyRecognitionBudget("code-878", "name-75");
        var view = LegacyRecognitionBudgetView.ToLegacyRecognitionBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyRecognitionBudgetView.ToLegacyRecognitionBudgetViews(new List<LegacyRecognitionBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyRecognitionBudgetViewShowsTheStatusInCapitals();
    }
}
