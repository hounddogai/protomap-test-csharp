namespace Acme.Workforce.Compensation.Tests.Adjustment.Schedule;

using Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AdjustmentVarianceChecks
{
    public static void AdjustmentVarianceStartsInCreated()
    {
        var item = AdjustmentVariance.NewAdjustmentVariance("code-212", "label-625");
        Check.Equal(item.Status, AdjustmentVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentVarianceMovesToValidated()
    {
        var item = AdjustmentVariance.NewAdjustmentVariance("code-212", "label-625");
        try
        {
            item.MoveTo(AdjustmentVarianceStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentVarianceStatus.Validated, "status");
    }

    public static void AdjustmentVarianceRejectsMovesFromRetired()
    {
        var item = AdjustmentVariance.NewAdjustmentVariance("code-212", "label-625");
        item.Status = AdjustmentVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(AdjustmentVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdjustmentVarianceStartsInCreated();
        AdjustmentVarianceMovesToValidated();
        AdjustmentVarianceRejectsMovesFromRetired();
    }
}
