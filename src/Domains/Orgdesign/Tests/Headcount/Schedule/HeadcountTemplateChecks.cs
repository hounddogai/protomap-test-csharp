namespace Acme.Workforce.Orgdesign.Tests.Headcount.Schedule;

using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class HeadcountTemplateChecks
{
    public static void HeadcountTemplateStartsInDraft()
    {
        var item = HeadcountTemplate.NewHeadcountTemplate("code-673", "title-312");
        Check.Equal(item.Status, HeadcountTemplateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountTemplateMovesToPending()
    {
        var item = HeadcountTemplate.NewHeadcountTemplate("code-673", "title-312");
        try
        {
            item.MoveTo(HeadcountTemplateStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountTemplateStatus.Pending, "status");
    }

    public static void HeadcountTemplateRejectsMovesFromClosed()
    {
        var item = HeadcountTemplate.NewHeadcountTemplate("code-673", "title-312");
        item.Status = HeadcountTemplateStatus.Closed;
        Check.Fails(() => item.MoveTo(HeadcountTemplateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        HeadcountTemplateStartsInDraft();
        HeadcountTemplateMovesToPending();
        HeadcountTemplateRejectsMovesFromClosed();
    }
}
