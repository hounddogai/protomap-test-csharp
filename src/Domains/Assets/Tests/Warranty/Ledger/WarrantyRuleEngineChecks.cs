namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class WarrantyRuleEngineChecks
{
    public static void WarrantyRuleEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyRuleEngine.DefaultWarrantyRuleEngine(Flags.NewRegistry());
        var item = WarrantyRule.NewWarrantyRule("code-431", "label-487");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyRuleEngineKeepsResultsWithinBounds();
    }
}
