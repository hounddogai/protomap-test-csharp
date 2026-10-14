namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentPlanChecks
{
    public static void AdaptiveGarnishmentPlanStartsInDraft()
    {
        var item = AdaptiveGarnishmentPlan.NewAdaptiveGarnishmentPlan("code-184", "title-607");
        Check.Equal(item.Status, AdaptiveGarnishmentPlanStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveGarnishmentPlanMovesToSuspended()
    {
        var item = AdaptiveGarnishmentPlan.NewAdaptiveGarnishmentPlan("code-184", "title-607");
        try
        {
            item.MoveTo(AdaptiveGarnishmentPlanStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveGarnishmentPlanStatus.Suspended, "status");
    }

    public static void AdaptiveGarnishmentPlanRejectsMovesFromClosed()
    {
        var item = AdaptiveGarnishmentPlan.NewAdaptiveGarnishmentPlan("code-184", "title-607");
        item.Status = AdaptiveGarnishmentPlanStatus.Closed;
        Check.Fails(() => item.MoveTo(AdaptiveGarnishmentPlanStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentPlanStartsInDraft();
        AdaptiveGarnishmentPlanMovesToSuspended();
        AdaptiveGarnishmentPlanRejectsMovesFromClosed();
    }
}
