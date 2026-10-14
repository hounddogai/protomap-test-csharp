namespace Acme.Workforce.Compensation.Tests.BonusPool.Tier;

using Acme.Workforce.Compensation.BonusPool.Tier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BonusPoolProposalEngineChecks
{
    public static void BonusPoolProposalEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolProposalEngine.DefaultBonusPoolProposalEngine(Flags.NewRegistry());
        var item = BonusPoolProposal.NewBonusPoolProposal("code-976", "title-549");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolProposalEngineKeepsResultsWithinBounds();
    }
}
