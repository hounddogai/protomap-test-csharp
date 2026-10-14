namespace Acme.Workforce.Talent.Tests.CareerPath.Exception;

using Acme.Workforce.Talent.CareerPath.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class BatchedCareerPathPlanChecks
{
    public static void BatchedCareerPathPlanStartsInCreated()
    {
        var item = BatchedCareerPathPlan.NewBatchedCareerPathPlan("code-596", "name-268");
        Check.Equal(item.Status, BatchedCareerPathPlanStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedCareerPathPlanMovesToValidated()
    {
        var item = BatchedCareerPathPlan.NewBatchedCareerPathPlan("code-596", "name-268");
        try
        {
            item.MoveTo(BatchedCareerPathPlanStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedCareerPathPlanStatus.Validated, "status");
    }

    public static void BatchedCareerPathPlanRejectsMovesFromRetired()
    {
        var item = BatchedCareerPathPlan.NewBatchedCareerPathPlan("code-596", "name-268");
        item.Status = BatchedCareerPathPlanStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedCareerPathPlanStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedCareerPathPlanStartsInCreated();
        BatchedCareerPathPlanMovesToValidated();
        BatchedCareerPathPlanRejectsMovesFromRetired();
    }
}
