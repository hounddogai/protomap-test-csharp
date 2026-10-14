namespace Acme.Workforce.Payrollops.Tests.Garnishment.Score;

using Acme.Workforce.Payrollops.Garnishment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class GarnishmentTargetChecks
{
    public static void GarnishmentTargetStartsInIdle()
    {
        var item = GarnishmentTarget.NewGarnishmentTarget("code-867", "title-78");
        Check.Equal(item.Status, GarnishmentTargetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentTargetMovesToDraining()
    {
        var item = GarnishmentTarget.NewGarnishmentTarget("code-867", "title-78");
        try
        {
            item.MoveTo(GarnishmentTargetStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentTargetStatus.Draining, "status");
    }

    public static void GarnishmentTargetRejectsMovesFromStopped()
    {
        var item = GarnishmentTarget.NewGarnishmentTarget("code-867", "title-78");
        item.Status = GarnishmentTargetStatus.Stopped;
        Check.Fails(() => item.MoveTo(GarnishmentTargetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GarnishmentTargetStartsInIdle();
        GarnishmentTargetMovesToDraining();
        GarnishmentTargetRejectsMovesFromStopped();
    }
}
