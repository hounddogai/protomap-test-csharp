namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientActionPlanRecordChecks
{
    public static void LenientActionPlanRecordStartsInPlanned()
    {
        var item = LenientActionPlanRecord.NewLenientActionPlanRecord("code-509", "name-170");
        Check.Equal(item.Status, LenientActionPlanRecordStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientActionPlanRecordMovesToCancelled()
    {
        var item = LenientActionPlanRecord.NewLenientActionPlanRecord("code-509", "name-170");
        try
        {
            item.MoveTo(LenientActionPlanRecordStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientActionPlanRecordStatus.Cancelled, "status");
    }

    public static void LenientActionPlanRecordRejectsMovesFromCancelled()
    {
        var item = LenientActionPlanRecord.NewLenientActionPlanRecord("code-509", "name-170");
        item.Status = LenientActionPlanRecordStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientActionPlanRecordStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientActionPlanRecordStartsInPlanned();
        LenientActionPlanRecordMovesToCancelled();
        LenientActionPlanRecordRejectsMovesFromCancelled();
    }
}
