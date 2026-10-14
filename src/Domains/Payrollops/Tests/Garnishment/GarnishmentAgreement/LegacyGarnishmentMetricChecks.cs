namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAgreement;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

/// <summary>Checks of garnishment agreement code, run by the project's runner.</summary>
public static class LegacyGarnishmentMetricChecks
{
    public static void LegacyGarnishmentMetricStartsInProposed()
    {
        var item = LegacyGarnishmentMetric.NewLegacyGarnishmentMetric("code-790", "label-64");
        Check.Equal(item.Status, LegacyGarnishmentMetricStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyGarnishmentMetricMovesToWithdrawn()
    {
        var item = LegacyGarnishmentMetric.NewLegacyGarnishmentMetric("code-790", "label-64");
        try
        {
            item.MoveTo(LegacyGarnishmentMetricStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyGarnishmentMetricStatus.Withdrawn, "status");
    }

    public static void LegacyGarnishmentMetricRejectsMovesFromWithdrawn()
    {
        var item = LegacyGarnishmentMetric.NewLegacyGarnishmentMetric("code-790", "label-64");
        item.Status = LegacyGarnishmentMetricStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyGarnishmentMetricStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyGarnishmentMetricStartsInProposed();
        LegacyGarnishmentMetricMovesToWithdrawn();
        LegacyGarnishmentMetricRejectsMovesFromWithdrawn();
    }
}
