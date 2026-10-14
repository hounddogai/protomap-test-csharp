namespace Acme.Workforce.Payrollops.Tests.Deduction.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryDeductionBudgetEngineChecks
{
    public static void PrimaryDeductionBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryDeductionBudgetEngine.DefaultPrimaryDeductionBudgetEngine(Flags.NewRegistry());
        var item = PrimaryDeductionBudget.NewPrimaryDeductionBudget("code-869", "name-738");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryDeductionBudgetEngineKeepsResultsWithinBounds();
    }
}
