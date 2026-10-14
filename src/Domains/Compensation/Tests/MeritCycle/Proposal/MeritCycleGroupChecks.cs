namespace Acme.Workforce.Compensation.Tests.MeritCycle.Proposal;

using Acme.Workforce.Compensation.MeritCycle.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class MeritCycleGroupChecks
{
    public static void MeritCycleGroupStartsInOpen()
    {
        var item = MeritCycleGroup.NewMeritCycleGroup("code-329", "name-936");
        Check.Equal(item.Status, MeritCycleGroupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleGroupMovesToInReview()
    {
        var item = MeritCycleGroup.NewMeritCycleGroup("code-329", "name-936");
        try
        {
            item.MoveTo(MeritCycleGroupStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleGroupStatus.InReview, "status");
    }

    public static void MeritCycleGroupRejectsMovesFromArchived()
    {
        var item = MeritCycleGroup.NewMeritCycleGroup("code-329", "name-936");
        item.Status = MeritCycleGroupStatus.Archived;
        Check.Fails(() => item.MoveTo(MeritCycleGroupStatus.Open), "move");
    }

    public static void RunAll()
    {
        MeritCycleGroupStartsInOpen();
        MeritCycleGroupMovesToInReview();
        MeritCycleGroupRejectsMovesFromArchived();
    }
}
