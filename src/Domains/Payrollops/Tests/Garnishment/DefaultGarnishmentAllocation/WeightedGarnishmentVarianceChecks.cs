namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class WeightedGarnishmentVarianceChecks
{
    public static void WeightedGarnishmentVarianceStartsInDraft()
    {
        var item = WeightedGarnishmentVariance.NewWeightedGarnishmentVariance("code-379", "title-522");
        Check.Equal(item.Status, WeightedGarnishmentVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedGarnishmentVarianceMovesToActive()
    {
        var item = WeightedGarnishmentVariance.NewWeightedGarnishmentVariance("code-379", "title-522");
        try
        {
            item.MoveTo(WeightedGarnishmentVarianceStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedGarnishmentVarianceStatus.Active, "status");
    }

    public static void WeightedGarnishmentVarianceRejectsMovesFromClosed()
    {
        var item = WeightedGarnishmentVariance.NewWeightedGarnishmentVariance("code-379", "title-522");
        item.Status = WeightedGarnishmentVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(WeightedGarnishmentVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WeightedGarnishmentVarianceStartsInDraft();
        WeightedGarnishmentVarianceMovesToActive();
        WeightedGarnishmentVarianceRejectsMovesFromClosed();
    }
}
