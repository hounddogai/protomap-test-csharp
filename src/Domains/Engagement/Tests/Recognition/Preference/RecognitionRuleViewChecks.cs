namespace Acme.Workforce.Engagement.Tests.Recognition.Preference;

using Acme.Workforce.Engagement.Recognition.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RecognitionRuleViewChecks
{
    public static void RecognitionRuleViewShowsTheStatusInCapitals()
    {
        var item = RecognitionRule.NewRecognitionRule("code-801", "title-973");
        var view = RecognitionRuleView.ToRecognitionRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionRuleView.ToRecognitionRuleViews(new List<RecognitionRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionRuleViewShowsTheStatusInCapitals();
    }
}
