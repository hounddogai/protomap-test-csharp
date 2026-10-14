namespace Acme.Workforce.Compliance.Tests.PolicyAck.Ledger;

using Acme.Workforce.Compliance.PolicyAck.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PolicyAckLedgerEngineChecks
{
    public static void PolicyAckLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckLedgerEngine.DefaultPolicyAckLedgerEngine(Flags.NewRegistry());
        var item = PolicyAckLedger.NewPolicyAckLedger("code-338", "name-479");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckLedgerEngineKeepsResultsWithinBounds();
    }
}
