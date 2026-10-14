namespace Acme.Workforce.Payrollops.Tests.Payslip.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class PayslipSnapshotEngineChecks
{
    public static void PayslipSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipSnapshotEngine.DefaultPayslipSnapshotEngine(Flags.NewRegistry());
        var item = PayslipSnapshot.NewPayslipSnapshot("code-677", "label-667");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipSnapshotEngineKeepsResultsWithinBounds();
    }
}
