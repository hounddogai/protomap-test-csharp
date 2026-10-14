namespace Acme.Workforce.Talent.Tests.CareerPath.Segment;

using Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CareerPathSegmentViewChecks
{
    public static void CareerPathSegmentViewShowsTheStatusInCapitals()
    {
        var item = CareerPathSegment.NewCareerPathSegment("code-991", "name-967");
        var view = CareerPathSegmentView.ToCareerPathSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathSegmentView.ToCareerPathSegmentViews(new List<CareerPathSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathSegmentViewShowsTheStatusInCapitals();
    }
}
