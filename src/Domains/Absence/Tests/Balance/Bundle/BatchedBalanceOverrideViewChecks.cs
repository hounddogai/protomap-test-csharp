namespace Acme.Workforce.Absence.Tests.Balance.Bundle;

using Acme.Workforce.Absence.Balance.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BatchedBalanceOverrideViewChecks
{
    public static void BatchedBalanceOverrideViewShowsTheStatusInCapitals()
    {
        var item = BatchedBalanceOverride.NewBatchedBalanceOverride("code-765", "name-953");
        var view = BatchedBalanceOverrideView.ToBatchedBalanceOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedBalanceOverrideView.ToBatchedBalanceOverrideViews(new List<BatchedBalanceOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedBalanceOverrideViewShowsTheStatusInCapitals();
    }
}
