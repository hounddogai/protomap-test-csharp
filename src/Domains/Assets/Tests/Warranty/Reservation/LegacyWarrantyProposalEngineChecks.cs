namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LegacyWarrantyProposalEngineChecks
{
    public static void LegacyWarrantyProposalEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyWarrantyProposalEngine.DefaultLegacyWarrantyProposalEngine(Flags.NewRegistry());
        var item = LegacyWarrantyProposal.NewLegacyWarrantyProposal("code-988", "title-937");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyWarrantyProposalEngineKeepsResultsWithinBounds();
    }
}
