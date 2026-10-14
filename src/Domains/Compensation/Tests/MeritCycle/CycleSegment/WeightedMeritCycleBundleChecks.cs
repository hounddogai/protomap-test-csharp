namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class WeightedMeritCycleBundleChecks
{
    public static void WeightedMeritCycleBundleStartsInOpen()
    {
        var item = WeightedMeritCycleBundle.NewWeightedMeritCycleBundle("code-408", "label-474");
        Check.Equal(item.Status, WeightedMeritCycleBundleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedMeritCycleBundleMovesToInReview()
    {
        var item = WeightedMeritCycleBundle.NewWeightedMeritCycleBundle("code-408", "label-474");
        try
        {
            item.MoveTo(WeightedMeritCycleBundleStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedMeritCycleBundleStatus.InReview, "status");
    }

    public static void WeightedMeritCycleBundleRejectsMovesFromArchived()
    {
        var item = WeightedMeritCycleBundle.NewWeightedMeritCycleBundle("code-408", "label-474");
        item.Status = WeightedMeritCycleBundleStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedMeritCycleBundleStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedMeritCycleBundleStartsInOpen();
        WeightedMeritCycleBundleMovesToInReview();
        WeightedMeritCycleBundleRejectsMovesFromArchived();
    }
}
