namespace Acme.Workforce.Orgdesign.Tests.Position.Mapping;

using Acme.Workforce.Orgdesign.Position.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PositionTemplateChecks
{
    public static void PositionTemplateStartsInOpen()
    {
        var item = PositionTemplate.NewPositionTemplate("code-474", "name-92");
        Check.Equal(item.Status, PositionTemplateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionTemplateMovesToReopened()
    {
        var item = PositionTemplate.NewPositionTemplate("code-474", "name-92");
        try
        {
            item.MoveTo(PositionTemplateStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionTemplateStatus.Reopened, "status");
    }

    public static void PositionTemplateRejectsMovesFromArchived()
    {
        var item = PositionTemplate.NewPositionTemplate("code-474", "name-92");
        item.Status = PositionTemplateStatus.Archived;
        Check.Fails(() => item.MoveTo(PositionTemplateStatus.Open), "move");
    }

    public static void RunAll()
    {
        PositionTemplateStartsInOpen();
        PositionTemplateMovesToReopened();
        PositionTemplateRejectsMovesFromArchived();
    }
}
