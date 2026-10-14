namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StrictAuditMetricChecks
{
    public static void StrictAuditMetricStartsInProposed()
    {
        var item = StrictAuditMetric.NewStrictAuditMetric("code-304", "label-234");
        Check.Equal(item.Status, StrictAuditMetricStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAuditMetricMovesToWithdrawn()
    {
        var item = StrictAuditMetric.NewStrictAuditMetric("code-304", "label-234");
        try
        {
            item.MoveTo(StrictAuditMetricStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAuditMetricStatus.Withdrawn, "status");
    }

    public static void StrictAuditMetricRejectsMovesFromWithdrawn()
    {
        var item = StrictAuditMetric.NewStrictAuditMetric("code-304", "label-234");
        item.Status = StrictAuditMetricStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictAuditMetricStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictAuditMetricStartsInProposed();
        StrictAuditMetricMovesToWithdrawn();
        StrictAuditMetricRejectsMovesFromWithdrawn();
    }
}
