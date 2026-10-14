namespace Acme.Workforce.Talent.Tests.CareerPath.Segment;

using Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CareerPathSegmentChecks
{
    public static void CareerPathSegmentStartsInPlanned()
    {
        var item = CareerPathSegment.NewCareerPathSegment("code-991", "name-967");
        Check.Equal(item.Status, CareerPathSegmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathSegmentMovesToCompleted()
    {
        var item = CareerPathSegment.NewCareerPathSegment("code-991", "name-967");
        try
        {
            item.MoveTo(CareerPathSegmentStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathSegmentStatus.Completed, "status");
    }

    public static void CareerPathSegmentRejectsMovesFromCancelled()
    {
        var item = CareerPathSegment.NewCareerPathSegment("code-991", "name-967");
        item.Status = CareerPathSegmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CareerPathSegmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CareerPathSegmentStartsInPlanned();
        CareerPathSegmentMovesToCompleted();
        CareerPathSegmentRejectsMovesFromCancelled();
    }
}
