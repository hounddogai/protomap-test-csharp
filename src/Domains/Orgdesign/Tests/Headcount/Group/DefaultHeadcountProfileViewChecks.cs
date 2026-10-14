namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class DefaultHeadcountProfileViewChecks
{
    public static void DefaultHeadcountProfileViewShowsTheStatusInCapitals()
    {
        var item = DefaultHeadcountProfile.NewDefaultHeadcountProfile("code-419", "label-523");
        var view = DefaultHeadcountProfileView.ToDefaultHeadcountProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultHeadcountProfileView.ToDefaultHeadcountProfileViews(new List<DefaultHeadcountProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultHeadcountProfileViewShowsTheStatusInCapitals();
    }
}
