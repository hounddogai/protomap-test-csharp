namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class RegionalWarrantyProposalEngineChecks
{
    public static void RegionalWarrantyProposalEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalWarrantyProposalEngine.DefaultRegionalWarrantyProposalEngine();
        var item = RegionalWarrantyProposal.NewRegionalWarrantyProposal("code-682", "name-99");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalWarrantyProposalEngineKeepsResultsWithinBounds();
    }
}
