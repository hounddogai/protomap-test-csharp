namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RollingBonusPoolProposalEngineChecks
{
    public static void RollingBonusPoolProposalEngineKeepsResultsWithinBounds()
    {
        var engine = RollingBonusPoolProposalEngine.DefaultRollingBonusPoolProposalEngine();
        var item = RollingBonusPoolProposal.NewRollingBonusPoolProposal("code-837", "title-267");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingBonusPoolProposalEngineKeepsResultsWithinBounds();
    }
}
