namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedHeadcountTemplateChecks
{
    public static void BatchedHeadcountTemplateStartsInNew()
    {
        var item = BatchedHeadcountTemplate.NewBatchedHeadcountTemplate("code-9", "name-529");
        Check.Equal(item.Status, BatchedHeadcountTemplateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedHeadcountTemplateMovesToFailed()
    {
        var item = BatchedHeadcountTemplate.NewBatchedHeadcountTemplate("code-9", "name-529");
        try
        {
            item.MoveTo(BatchedHeadcountTemplateStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedHeadcountTemplateStatus.Failed, "status");
    }

    public static void BatchedHeadcountTemplateRejectsMovesFromFailed()
    {
        var item = BatchedHeadcountTemplate.NewBatchedHeadcountTemplate("code-9", "name-529");
        item.Status = BatchedHeadcountTemplateStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedHeadcountTemplateStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedHeadcountTemplateStartsInNew();
        BatchedHeadcountTemplateMovesToFailed();
        BatchedHeadcountTemplateRejectsMovesFromFailed();
    }
}
