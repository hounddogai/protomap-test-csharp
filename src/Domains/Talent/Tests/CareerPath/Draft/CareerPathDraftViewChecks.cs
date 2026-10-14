namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CareerPathDraftViewChecks
{
    public static void CareerPathDraftViewShowsTheStatusInCapitals()
    {
        var item = CareerPathDraft.NewCareerPathDraft("code-538", "label-879");
        var view = CareerPathDraftView.ToCareerPathDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathDraftView.ToCareerPathDraftViews(new List<CareerPathDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathDraftViewShowsTheStatusInCapitals();
    }
}
