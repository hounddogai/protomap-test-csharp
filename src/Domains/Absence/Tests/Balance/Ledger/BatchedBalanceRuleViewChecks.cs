namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedBalanceRuleViewChecks
{
    public static void BatchedBalanceRuleViewShowsTheStatusInCapitals()
    {
        var item = BatchedBalanceRule.NewBatchedBalanceRule("code-591", "name-72");
        var view = BatchedBalanceRuleView.ToBatchedBalanceRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedBalanceRuleView.ToBatchedBalanceRuleViews(new List<BatchedBalanceRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedBalanceRuleViewShowsTheStatusInCapitals();
    }
}
