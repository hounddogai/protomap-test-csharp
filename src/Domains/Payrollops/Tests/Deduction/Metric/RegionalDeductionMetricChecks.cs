namespace Acme.Workforce.Payrollops.Tests.Deduction.Metric;

using Acme.Workforce.Payrollops.Deduction.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class RegionalDeductionMetricChecks
{
    public static void RegionalDeductionMetricStartsInCreated()
    {
        var item = RegionalDeductionMetric.NewRegionalDeductionMetric("code-333", "label-166");
        Check.Equal(item.Status, RegionalDeductionMetricStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalDeductionMetricMovesToValidated()
    {
        var item = RegionalDeductionMetric.NewRegionalDeductionMetric("code-333", "label-166");
        try
        {
            item.MoveTo(RegionalDeductionMetricStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalDeductionMetricStatus.Validated, "status");
    }

    public static void RegionalDeductionMetricRejectsMovesFromRetired()
    {
        var item = RegionalDeductionMetric.NewRegionalDeductionMetric("code-333", "label-166");
        item.Status = RegionalDeductionMetricStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalDeductionMetricStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalDeductionMetricStartsInCreated();
        RegionalDeductionMetricMovesToValidated();
        RegionalDeductionMetricRejectsMovesFromRetired();
    }
}
