namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LegacyActionPlanMetricChecks
{
    public static void LegacyActionPlanMetricStartsInRequested()
    {
        var item = LegacyActionPlanMetric.NewLegacyActionPlanMetric("code-891", "label-979");
        Check.Equal(item.Status, LegacyActionPlanMetricStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyActionPlanMetricMovesToGranted()
    {
        var item = LegacyActionPlanMetric.NewLegacyActionPlanMetric("code-891", "label-979");
        try
        {
            item.MoveTo(LegacyActionPlanMetricStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyActionPlanMetricStatus.Granted, "status");
    }

    public static void LegacyActionPlanMetricRejectsMovesFromExpired()
    {
        var item = LegacyActionPlanMetric.NewLegacyActionPlanMetric("code-891", "label-979");
        item.Status = LegacyActionPlanMetricStatus.Expired;
        Check.Fails(() => item.MoveTo(LegacyActionPlanMetricStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LegacyActionPlanMetricStartsInRequested();
        LegacyActionPlanMetricMovesToGranted();
        LegacyActionPlanMetricRejectsMovesFromExpired();
    }
}
