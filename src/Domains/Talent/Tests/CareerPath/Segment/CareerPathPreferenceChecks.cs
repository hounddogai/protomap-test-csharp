namespace Acme.Workforce.Talent.Tests.CareerPath.Segment;

using Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CareerPathPreferenceChecks
{
    public static void CareerPathPreferenceStartsInPlanned()
    {
        var item = CareerPathPreference.NewCareerPathPreference("code-696", "title-711");
        Check.Equal(item.Status, CareerPathPreferenceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CareerPathPreferenceMovesToInProgress()
    {
        var item = CareerPathPreference.NewCareerPathPreference("code-696", "title-711");
        try
        {
            item.MoveTo(CareerPathPreferenceStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CareerPathPreferenceStatus.InProgress, "status");
    }

    public static void CareerPathPreferenceRejectsMovesFromCancelled()
    {
        var item = CareerPathPreference.NewCareerPathPreference("code-696", "title-711");
        item.Status = CareerPathPreferenceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CareerPathPreferenceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CareerPathPreferenceStartsInPlanned();
        CareerPathPreferenceMovesToInProgress();
        CareerPathPreferenceRejectsMovesFromCancelled();
    }
}
