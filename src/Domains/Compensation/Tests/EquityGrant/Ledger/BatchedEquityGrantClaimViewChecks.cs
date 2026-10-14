namespace Acme.Workforce.Compensation.Tests.EquityGrant.Ledger;

using Acme.Workforce.Compensation.EquityGrant.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedEquityGrantClaimViewChecks
{
    public static void BatchedEquityGrantClaimViewShowsTheStatusInCapitals()
    {
        var item = BatchedEquityGrantClaim.NewBatchedEquityGrantClaim("code-160", "title-31");
        var view = BatchedEquityGrantClaimView.ToBatchedEquityGrantClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedEquityGrantClaimView.ToBatchedEquityGrantClaimViews(new List<BatchedEquityGrantClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedEquityGrantClaimViewShowsTheStatusInCapitals();
    }
}
