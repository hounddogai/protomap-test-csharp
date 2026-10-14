namespace Acme.Workforce.Compensation.Tests.MeritCycle.Draft;

using Acme.Workforce.Compensation.MeritCycle.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class MeritCycleTemplateChecks
{
    public static void MeritCycleTemplateStartsInProposed()
    {
        var item = MeritCycleTemplate.NewMeritCycleTemplate("code-941", "title-975");
        Check.Equal(item.Status, MeritCycleTemplateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleTemplateMovesToWithdrawn()
    {
        var item = MeritCycleTemplate.NewMeritCycleTemplate("code-941", "title-975");
        try
        {
            item.MoveTo(MeritCycleTemplateStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleTemplateStatus.Withdrawn, "status");
    }

    public static void MeritCycleTemplateRejectsMovesFromWithdrawn()
    {
        var item = MeritCycleTemplate.NewMeritCycleTemplate("code-941", "title-975");
        item.Status = MeritCycleTemplateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(MeritCycleTemplateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        MeritCycleTemplateStartsInProposed();
        MeritCycleTemplateMovesToWithdrawn();
        MeritCycleTemplateRejectsMovesFromWithdrawn();
    }
}
