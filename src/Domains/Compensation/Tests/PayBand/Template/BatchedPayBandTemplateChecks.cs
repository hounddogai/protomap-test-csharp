namespace Acme.Workforce.Compensation.Tests.PayBand.Template;

using Acme.Workforce.Compensation.PayBand.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class BatchedPayBandTemplateChecks
{
    public static void BatchedPayBandTemplateStartsInOpen()
    {
        var item = BatchedPayBandTemplate.NewBatchedPayBandTemplate("code-271", "label-609");
        Check.Equal(item.Status, BatchedPayBandTemplateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedPayBandTemplateMovesToInReview()
    {
        var item = BatchedPayBandTemplate.NewBatchedPayBandTemplate("code-271", "label-609");
        try
        {
            item.MoveTo(BatchedPayBandTemplateStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedPayBandTemplateStatus.InReview, "status");
    }

    public static void BatchedPayBandTemplateRejectsMovesFromArchived()
    {
        var item = BatchedPayBandTemplate.NewBatchedPayBandTemplate("code-271", "label-609");
        item.Status = BatchedPayBandTemplateStatus.Archived;
        Check.Fails(() => item.MoveTo(BatchedPayBandTemplateStatus.Open), "move");
    }

    public static void RunAll()
    {
        BatchedPayBandTemplateStartsInOpen();
        BatchedPayBandTemplateMovesToInReview();
        BatchedPayBandTemplateRejectsMovesFromArchived();
    }
}
