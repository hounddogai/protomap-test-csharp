namespace Acme.Workforce.Assets.Tests.Assignment.Batch;

using Acme.Workforce.Assets.Assignment.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DefaultAssignmentBatchChecks
{
    public static void DefaultAssignmentBatchStartsInCreated()
    {
        var item = DefaultAssignmentBatch.NewDefaultAssignmentBatch("code-872", "name-195");
        Check.Equal(item.Status, DefaultAssignmentBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAssignmentBatchMovesToValidated()
    {
        var item = DefaultAssignmentBatch.NewDefaultAssignmentBatch("code-872", "name-195");
        try
        {
            item.MoveTo(DefaultAssignmentBatchStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAssignmentBatchStatus.Validated, "status");
    }

    public static void DefaultAssignmentBatchRejectsMovesFromRetired()
    {
        var item = DefaultAssignmentBatch.NewDefaultAssignmentBatch("code-872", "name-195");
        item.Status = DefaultAssignmentBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(DefaultAssignmentBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        DefaultAssignmentBatchStartsInCreated();
        DefaultAssignmentBatchMovesToValidated();
        DefaultAssignmentBatchRejectsMovesFromRetired();
    }
}
