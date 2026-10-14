namespace Acme.Workforce.Assets.Tests.Laptop.Rollup;

using Acme.Workforce.Assets.Laptop.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BatchedLaptopTemplateChecks
{
    public static void BatchedLaptopTemplateStartsInDraft()
    {
        var item = BatchedLaptopTemplate.NewBatchedLaptopTemplate("code-204", "title-639");
        Check.Equal(item.Status, BatchedLaptopTemplateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedLaptopTemplateMovesToPending()
    {
        var item = BatchedLaptopTemplate.NewBatchedLaptopTemplate("code-204", "title-639");
        try
        {
            item.MoveTo(BatchedLaptopTemplateStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedLaptopTemplateStatus.Pending, "status");
    }

    public static void BatchedLaptopTemplateRejectsMovesFromClosed()
    {
        var item = BatchedLaptopTemplate.NewBatchedLaptopTemplate("code-204", "title-639");
        item.Status = BatchedLaptopTemplateStatus.Closed;
        Check.Fails(() => item.MoveTo(BatchedLaptopTemplateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BatchedLaptopTemplateStartsInDraft();
        BatchedLaptopTemplateMovesToPending();
        BatchedLaptopTemplateRejectsMovesFromClosed();
    }
}
