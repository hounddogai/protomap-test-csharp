namespace Acme.Workforce.Talent.Tests.CareerPath.Agreement;

using Acme.Workforce.Talent.CareerPath.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CareerPathManifestChecks
{
    public static void CareerPathManifestStartsInPlanned()
    {
        var item = CareerPathManifest.NewCareerPathManifest("code-597", "title-514");
        Check.Equal(item.Status, CareerPathManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathManifestMovesToScheduled()
    {
        var item = CareerPathManifest.NewCareerPathManifest("code-597", "title-514");
        try
        {
            item.MoveTo(CareerPathManifestStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathManifestStatus.Scheduled, "status");
    }

    public static void CareerPathManifestRejectsMovesFromCancelled()
    {
        var item = CareerPathManifest.NewCareerPathManifest("code-597", "title-514");
        item.Status = CareerPathManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CareerPathManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CareerPathManifestStartsInPlanned();
        CareerPathManifestMovesToScheduled();
        CareerPathManifestRejectsMovesFromCancelled();
    }
}
