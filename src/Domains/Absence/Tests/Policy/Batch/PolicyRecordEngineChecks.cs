namespace Acme.Workforce.Absence.Tests.Policy.Batch;

using Acme.Workforce.Absence.Policy.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PolicyRecordEngineChecks
{
    public static void PolicyRecordEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyRecordEngine.DefaultPolicyRecordEngine(Flags.NewRegistry());
        var item = PolicyRecord.NewPolicyRecord("code-846", "label-977");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyRecordEngineKeepsResultsWithinBounds();
    }
}
