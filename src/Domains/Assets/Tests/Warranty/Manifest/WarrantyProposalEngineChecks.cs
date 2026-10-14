namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyProposalEngineChecks
{
    public static void WarrantyProposalEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyProposalEngine.DefaultWarrantyProposalEngine();
        var item = WarrantyProposal.NewWarrantyProposal("code-331", "title-473");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyProposalEngineKeepsResultsWithinBounds();
    }
}
