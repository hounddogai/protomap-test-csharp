namespace Acme.Workforce.Talent.Tests.CareerPath.PathSummary;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Checks of path summary code, run by the project's runner.</summary>
public static class ManualCareerPathOverrideChecks
{
    public static void ManualCareerPathOverrideStartsInPlanned()
    {
        var item = ManualCareerPathOverride.NewManualCareerPathOverride("code-859", "name-758");
        Check.Equal(item.Status, ManualCareerPathOverrideStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualCareerPathOverrideMovesToInProgress()
    {
        var item = ManualCareerPathOverride.NewManualCareerPathOverride("code-859", "name-758");
        try
        {
            item.MoveTo(ManualCareerPathOverrideStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualCareerPathOverrideStatus.InProgress, "status");
    }

    public static void ManualCareerPathOverrideRejectsMovesFromCancelled()
    {
        var item = ManualCareerPathOverride.NewManualCareerPathOverride("code-859", "name-758");
        item.Status = ManualCareerPathOverrideStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ManualCareerPathOverrideStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ManualCareerPathOverrideStartsInPlanned();
        ManualCareerPathOverrideMovesToInProgress();
        ManualCareerPathOverrideRejectsMovesFromCancelled();
    }
}
