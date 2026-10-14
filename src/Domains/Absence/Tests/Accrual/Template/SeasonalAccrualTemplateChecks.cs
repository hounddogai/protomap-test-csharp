namespace Acme.Workforce.Absence.Tests.Accrual.Template;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SeasonalAccrualTemplateChecks
{
    public static void SeasonalAccrualTemplateStartsInDraft()
    {
        var item = SeasonalAccrualTemplate.NewSeasonalAccrualTemplate("code-552", "name-482");
        Check.Equal(item.Status, SeasonalAccrualTemplateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAccrualTemplateMovesToPending()
    {
        var item = SeasonalAccrualTemplate.NewSeasonalAccrualTemplate("code-552", "name-482");
        try
        {
            item.MoveTo(SeasonalAccrualTemplateStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAccrualTemplateStatus.Pending, "status");
    }

    public static void SeasonalAccrualTemplateRejectsMovesFromClosed()
    {
        var item = SeasonalAccrualTemplate.NewSeasonalAccrualTemplate("code-552", "name-482");
        item.Status = SeasonalAccrualTemplateStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalAccrualTemplateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalAccrualTemplateStartsInDraft();
        SeasonalAccrualTemplateMovesToPending();
        SeasonalAccrualTemplateRejectsMovesFromClosed();
    }
}
