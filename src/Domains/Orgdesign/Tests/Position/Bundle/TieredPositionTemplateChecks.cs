namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredPositionTemplateChecks
{
    public static void TieredPositionTemplateStartsInDraft()
    {
        var item = TieredPositionTemplate.NewTieredPositionTemplate("code-381", "label-484");
        Check.Equal(item.Status, TieredPositionTemplateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPositionTemplateMovesToPending()
    {
        var item = TieredPositionTemplate.NewTieredPositionTemplate("code-381", "label-484");
        try
        {
            item.MoveTo(TieredPositionTemplateStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPositionTemplateStatus.Pending, "status");
    }

    public static void TieredPositionTemplateRejectsMovesFromClosed()
    {
        var item = TieredPositionTemplate.NewTieredPositionTemplate("code-381", "label-484");
        item.Status = TieredPositionTemplateStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredPositionTemplateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredPositionTemplateStartsInDraft();
        TieredPositionTemplateMovesToPending();
        TieredPositionTemplateRejectsMovesFromClosed();
    }
}
