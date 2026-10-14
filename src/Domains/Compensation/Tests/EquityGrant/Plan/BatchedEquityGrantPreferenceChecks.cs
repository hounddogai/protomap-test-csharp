namespace Acme.Workforce.Compensation.Tests.EquityGrant.Plan;

using Acme.Workforce.Compensation.EquityGrant.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BatchedEquityGrantPreferenceChecks
{
    public static void BatchedEquityGrantPreferenceStartsInCreated()
    {
        var item = BatchedEquityGrantPreference.NewBatchedEquityGrantPreference("code-665", "name-540");
        Check.Equal(item.Status, BatchedEquityGrantPreferenceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedEquityGrantPreferenceMovesToValidated()
    {
        var item = BatchedEquityGrantPreference.NewBatchedEquityGrantPreference("code-665", "name-540");
        try
        {
            item.MoveTo(BatchedEquityGrantPreferenceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedEquityGrantPreferenceStatus.Validated, "status");
    }

    public static void BatchedEquityGrantPreferenceRejectsMovesFromRetired()
    {
        var item = BatchedEquityGrantPreference.NewBatchedEquityGrantPreference("code-665", "name-540");
        item.Status = BatchedEquityGrantPreferenceStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedEquityGrantPreferenceStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedEquityGrantPreferenceStartsInCreated();
        BatchedEquityGrantPreferenceMovesToValidated();
        BatchedEquityGrantPreferenceRejectsMovesFromRetired();
    }
}
