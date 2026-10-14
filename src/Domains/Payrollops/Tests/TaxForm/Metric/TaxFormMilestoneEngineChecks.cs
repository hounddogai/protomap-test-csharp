namespace Acme.Workforce.Payrollops.Tests.TaxForm.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TaxFormMilestoneEngineChecks
{
    public static void TaxFormMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormMilestoneEngine.DefaultTaxFormMilestoneEngine(Flags.NewRegistry());
        var item = TaxFormMilestone.NewTaxFormMilestone("code-249", "title-929");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormMilestoneEngineKeepsResultsWithinBounds();
    }
}
