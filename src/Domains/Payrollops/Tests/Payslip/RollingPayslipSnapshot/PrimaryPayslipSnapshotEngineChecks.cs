namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class PrimaryPayslipSnapshotEngineChecks
{
    public static void PrimaryPayslipSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryPayslipSnapshotEngine.DefaultPrimaryPayslipSnapshotEngine(Flags.NewRegistry());
        var item = PrimaryPayslipSnapshot.NewPrimaryPayslipSnapshot("code-995", "label-152");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryPayslipSnapshotEngineKeepsResultsWithinBounds();
    }
}
