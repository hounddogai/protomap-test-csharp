namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class BatchedPoolAuditViewChecks
{
    public static void BatchedPoolAuditViewShowsTheStatusInCapitals()
    {
        var item = BatchedPoolAudit.NewBatchedPoolAudit("code-782", "name-650");
        var view = BatchedPoolAuditView.ToBatchedPoolAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedPoolAuditView.ToBatchedPoolAuditViews(new List<BatchedPoolAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedPoolAuditViewShowsTheStatusInCapitals();
    }
}
