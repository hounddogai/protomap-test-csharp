namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledPositionTemplateChecks
{
    public static void ScheduledPositionTemplateStartsInIdle()
    {
        var item = ScheduledPositionTemplate.NewScheduledPositionTemplate("code-222", "title-66");
        Check.Equal(item.Status, ScheduledPositionTemplateStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPositionTemplateMovesToReady()
    {
        var item = ScheduledPositionTemplate.NewScheduledPositionTemplate("code-222", "title-66");
        try
        {
            item.MoveTo(ScheduledPositionTemplateStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPositionTemplateStatus.Ready, "status");
    }

    public static void ScheduledPositionTemplateRejectsMovesFromStopped()
    {
        var item = ScheduledPositionTemplate.NewScheduledPositionTemplate("code-222", "title-66");
        item.Status = ScheduledPositionTemplateStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledPositionTemplateStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledPositionTemplateStartsInIdle();
        ScheduledPositionTemplateMovesToReady();
        ScheduledPositionTemplateRejectsMovesFromStopped();
    }
}
