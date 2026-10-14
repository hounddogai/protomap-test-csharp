namespace Acme.Workforce.Payrollops.Tests.Garnishment.Entry;

using Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FallbackGarnishmentPlanChecks
{
    public static void FallbackGarnishmentPlanStartsInNew()
    {
        var item = FallbackGarnishmentPlan.NewFallbackGarnishmentPlan("code-790", "title-535");
        Check.Equal(item.Status, FallbackGarnishmentPlanStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackGarnishmentPlanMovesToFailed()
    {
        var item = FallbackGarnishmentPlan.NewFallbackGarnishmentPlan("code-790", "title-535");
        try
        {
            item.MoveTo(FallbackGarnishmentPlanStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackGarnishmentPlanStatus.Failed, "status");
    }

    public static void FallbackGarnishmentPlanRejectsMovesFromFailed()
    {
        var item = FallbackGarnishmentPlan.NewFallbackGarnishmentPlan("code-790", "title-535");
        item.Status = FallbackGarnishmentPlanStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackGarnishmentPlanStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackGarnishmentPlanStartsInNew();
        FallbackGarnishmentPlanMovesToFailed();
        FallbackGarnishmentPlanRejectsMovesFromFailed();
    }
}
