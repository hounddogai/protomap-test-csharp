namespace Acme.Workforce.Talent.Tests.CareerPath.Review;

using Acme.Workforce.Talent.CareerPath.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CareerPathAssignmentViewChecks
{
    public static void CareerPathAssignmentViewShowsTheStatusInCapitals()
    {
        var item = CareerPathAssignment.NewCareerPathAssignment("code-607", "name-71");
        var view = CareerPathAssignmentView.ToCareerPathAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CareerPathAssignmentView.ToCareerPathAssignmentViews(new List<CareerPathAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CareerPathAssignmentViewShowsTheStatusInCapitals();
    }
}
