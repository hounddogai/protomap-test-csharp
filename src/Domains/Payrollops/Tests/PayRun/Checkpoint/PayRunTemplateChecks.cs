namespace Acme.Workforce.Payrollops.Tests.PayRun.Checkpoint;

using Acme.Workforce.Payrollops.PayRun.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PayRunTemplateChecks
{
    public static void PayRunTemplateStartsInPlanned()
    {
        var item = PayRunTemplate.NewPayRunTemplate("code-809", "label-301");
        Check.Equal(item.Status, PayRunTemplateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunTemplateMovesToInProgress()
    {
        var item = PayRunTemplate.NewPayRunTemplate("code-809", "label-301");
        try
        {
            item.MoveTo(PayRunTemplateStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunTemplateStatus.InProgress, "status");
    }

    public static void PayRunTemplateRejectsMovesFromCancelled()
    {
        var item = PayRunTemplate.NewPayRunTemplate("code-809", "label-301");
        item.Status = PayRunTemplateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PayRunTemplateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PayRunTemplateStartsInPlanned();
        PayRunTemplateMovesToInProgress();
        PayRunTemplateRejectsMovesFromCancelled();
    }
}
