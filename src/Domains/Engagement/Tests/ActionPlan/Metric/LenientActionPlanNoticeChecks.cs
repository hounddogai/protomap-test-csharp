namespace Acme.Workforce.Engagement.Tests.ActionPlan.Metric;

using Acme.Workforce.Engagement.ActionPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LenientActionPlanNoticeChecks
{
    public static void LenientActionPlanNoticeStartsInOpen()
    {
        var item = LenientActionPlanNotice.NewLenientActionPlanNotice("code-73", "label-762");
        Check.Equal(item.Status, LenientActionPlanNoticeStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientActionPlanNoticeMovesToResolved()
    {
        var item = LenientActionPlanNotice.NewLenientActionPlanNotice("code-73", "label-762");
        try
        {
            item.MoveTo(LenientActionPlanNoticeStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientActionPlanNoticeStatus.Resolved, "status");
    }

    public static void LenientActionPlanNoticeRejectsMovesFromArchived()
    {
        var item = LenientActionPlanNotice.NewLenientActionPlanNotice("code-73", "label-762");
        item.Status = LenientActionPlanNoticeStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientActionPlanNoticeStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientActionPlanNoticeStartsInOpen();
        LenientActionPlanNoticeMovesToResolved();
        LenientActionPlanNoticeRejectsMovesFromArchived();
    }
}
