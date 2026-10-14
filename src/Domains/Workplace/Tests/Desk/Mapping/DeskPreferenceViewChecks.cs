namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskPreferenceViewChecks
{
    public static void DeskPreferenceViewShowsTheStatusInCapitals()
    {
        var item = DeskPreference.NewDeskPreference("code-865", "label-796");
        var view = DeskPreferenceView.ToDeskPreferenceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskPreferenceView.ToDeskPreferenceViews(new List<DeskPreference> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskPreferenceViewShowsTheStatusInCapitals();
    }
}
