namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LegacyPositionTemplateChecks
{
    public static void LegacyPositionTemplateStartsInNew()
    {
        var item = LegacyPositionTemplate.NewLegacyPositionTemplate("code-371", "name-815");
        Check.Equal(item.Status, LegacyPositionTemplateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPositionTemplateMovesToRunning()
    {
        var item = LegacyPositionTemplate.NewLegacyPositionTemplate("code-371", "name-815");
        try
        {
            item.MoveTo(LegacyPositionTemplateStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPositionTemplateStatus.Running, "status");
    }

    public static void LegacyPositionTemplateRejectsMovesFromFailed()
    {
        var item = LegacyPositionTemplate.NewLegacyPositionTemplate("code-371", "name-815");
        item.Status = LegacyPositionTemplateStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyPositionTemplateStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyPositionTemplateStartsInNew();
        LegacyPositionTemplateMovesToRunning();
        LegacyPositionTemplateRejectsMovesFromFailed();
    }
}
