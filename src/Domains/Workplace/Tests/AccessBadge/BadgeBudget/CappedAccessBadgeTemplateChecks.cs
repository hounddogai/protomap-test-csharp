namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class CappedAccessBadgeTemplateChecks
{
    public static void CappedAccessBadgeTemplateStartsInPlanned()
    {
        var item = CappedAccessBadgeTemplate.NewCappedAccessBadgeTemplate("code-50", "title-626");
        Check.Equal(item.Status, CappedAccessBadgeTemplateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAccessBadgeTemplateMovesToInProgress()
    {
        var item = CappedAccessBadgeTemplate.NewCappedAccessBadgeTemplate("code-50", "title-626");
        try
        {
            item.MoveTo(CappedAccessBadgeTemplateStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAccessBadgeTemplateStatus.InProgress, "status");
    }

    public static void CappedAccessBadgeTemplateRejectsMovesFromCancelled()
    {
        var item = CappedAccessBadgeTemplate.NewCappedAccessBadgeTemplate("code-50", "title-626");
        item.Status = CappedAccessBadgeTemplateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(CappedAccessBadgeTemplateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        CappedAccessBadgeTemplateStartsInPlanned();
        CappedAccessBadgeTemplateMovesToInProgress();
        CappedAccessBadgeTemplateRejectsMovesFromCancelled();
    }
}
