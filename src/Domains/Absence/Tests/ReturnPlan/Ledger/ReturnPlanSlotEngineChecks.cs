namespace Acme.Workforce.Absence.Tests.ReturnPlan.Ledger;

using Acme.Workforce.Absence.ReturnPlan.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ReturnPlanSlotEngineChecks
{
    public static void ReturnPlanSlotEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanSlotEngine.DefaultReturnPlanSlotEngine(Flags.NewRegistry());
        var item = ReturnPlanSlot.NewReturnPlanSlot("code-646", "label-425");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanSlotEngineKeepsResultsWithinBounds();
    }
}
