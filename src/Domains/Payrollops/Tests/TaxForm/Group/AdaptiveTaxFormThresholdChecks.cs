namespace Acme.Workforce.Payrollops.Tests.TaxForm.Group;

using Acme.Workforce.Payrollops.TaxForm.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class AdaptiveTaxFormThresholdChecks
{
    public static void AdaptiveTaxFormThresholdStartsInPlanned()
    {
        var item = AdaptiveTaxFormThreshold.NewAdaptiveTaxFormThreshold("code-426", "name-162");
        Check.Equal(item.Status, AdaptiveTaxFormThresholdStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveTaxFormThresholdMovesToCancelled()
    {
        var item = AdaptiveTaxFormThreshold.NewAdaptiveTaxFormThreshold("code-426", "name-162");
        try
        {
            item.MoveTo(AdaptiveTaxFormThresholdStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveTaxFormThresholdStatus.Cancelled, "status");
    }

    public static void AdaptiveTaxFormThresholdRejectsMovesFromCancelled()
    {
        var item = AdaptiveTaxFormThreshold.NewAdaptiveTaxFormThreshold("code-426", "name-162");
        item.Status = AdaptiveTaxFormThresholdStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveTaxFormThresholdStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveTaxFormThresholdStartsInPlanned();
        AdaptiveTaxFormThresholdMovesToCancelled();
        AdaptiveTaxFormThresholdRejectsMovesFromCancelled();
    }
}
