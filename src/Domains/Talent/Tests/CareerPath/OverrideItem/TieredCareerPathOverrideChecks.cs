namespace Acme.Workforce.Talent.Tests.CareerPath.OverrideItem;

using Acme.Workforce.Talent.CareerPath.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TieredCareerPathOverrideChecks
{
    public static void TieredCareerPathOverrideStartsInPlanned()
    {
        var item = TieredCareerPathOverride.NewTieredCareerPathOverride("code-643", "label-595");
        Check.Equal(item.Status, TieredCareerPathOverrideStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredCareerPathOverrideMovesToInProgress()
    {
        var item = TieredCareerPathOverride.NewTieredCareerPathOverride("code-643", "label-595");
        try
        {
            item.MoveTo(TieredCareerPathOverrideStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredCareerPathOverrideStatus.InProgress, "status");
    }

    public static void TieredCareerPathOverrideRejectsMovesFromCancelled()
    {
        var item = TieredCareerPathOverride.NewTieredCareerPathOverride("code-643", "label-595");
        item.Status = TieredCareerPathOverrideStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredCareerPathOverrideStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredCareerPathOverrideStartsInPlanned();
        TieredCareerPathOverrideMovesToInProgress();
        TieredCareerPathOverrideRejectsMovesFromCancelled();
    }
}
