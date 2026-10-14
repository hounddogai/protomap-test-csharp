namespace Acme.Workforce.Talent.Tests.CareerPath.Variance;

using Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class CareerPathTemplateViewChecks
{
    public static void CareerPathTemplateViewShowsTheStatusInCapitals()
    {
        var item = CareerPathTemplate.NewCareerPathTemplate("code-571", "name-880");
        var view = CareerPathTemplateView.ToCareerPathTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathTemplateView.ToCareerPathTemplateViews(new List<CareerPathTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathTemplateViewShowsTheStatusInCapitals();
    }
}
