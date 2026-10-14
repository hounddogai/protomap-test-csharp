namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReorgPreferenceViewChecks
{
    public static void ReorgPreferenceViewShowsTheStatusInCapitals()
    {
        var item = ReorgPreference.NewReorgPreference("code-783", "title-96");
        var view = ReorgPreferenceView.ToReorgPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgPreferenceView.ToReorgPreferenceViews(new List<ReorgPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgPreferenceViewShowsTheStatusInCapitals();
    }
}
