namespace Acme.Workforce.Compliance.Tests.Audit.Bundle;

using Acme.Workforce.Compliance.Audit.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WeightedAuditMetricChecks
{
    public static void WeightedAuditMetricStartsInProposed()
    {
        var item = WeightedAuditMetric.NewWeightedAuditMetric("code-336", "title-696");
        Check.Equal(item.Status, WeightedAuditMetricStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAuditMetricMovesToWithdrawn()
    {
        var item = WeightedAuditMetric.NewWeightedAuditMetric("code-336", "title-696");
        try
        {
            item.MoveTo(WeightedAuditMetricStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAuditMetricStatus.Withdrawn, "status");
    }

    public static void WeightedAuditMetricRejectsMovesFromWithdrawn()
    {
        var item = WeightedAuditMetric.NewWeightedAuditMetric("code-336", "title-696");
        item.Status = WeightedAuditMetricStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedAuditMetricStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedAuditMetricStartsInProposed();
        WeightedAuditMetricMovesToWithdrawn();
        WeightedAuditMetricRejectsMovesFromWithdrawn();
    }
}
