namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallTemplateChecks
{
    public static void TownHallTemplateStartsInNew()
    {
        var item = TownHallTemplate.NewTownHallTemplate("code-796", "label-155");
        Check.Equal(item.Status, TownHallTemplateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallTemplateMovesToQueued()
    {
        var item = TownHallTemplate.NewTownHallTemplate("code-796", "label-155");
        try
        {
            item.MoveTo(TownHallTemplateStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallTemplateStatus.Queued, "status");
    }

    public static void TownHallTemplateRejectsMovesFromFailed()
    {
        var item = TownHallTemplate.NewTownHallTemplate("code-796", "label-155");
        item.Status = TownHallTemplateStatus.Failed;
        Check.Fails(() => item.MoveTo(TownHallTemplateStatus.New), "move");
    }

    public static void RunAll()
    {
        TownHallTemplateStartsInNew();
        TownHallTemplateMovesToQueued();
        TownHallTemplateRejectsMovesFromFailed();
    }
}
