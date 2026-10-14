namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class FallbackAccessBadgeAgreementEngineChecks
{
    public static void FallbackAccessBadgeAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackAccessBadgeAgreementEngine.DefaultFallbackAccessBadgeAgreementEngine(Flags.NewRegistry());
        var item = FallbackAccessBadgeAgreement.NewFallbackAccessBadgeAgreement("code-852", "title-277");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackAccessBadgeAgreementEngineKeepsResultsWithinBounds();
    }
}
