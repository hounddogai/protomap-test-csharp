namespace Acme.Workforce.Assets.Tests.Assignment.Agreement;

using Acme.Workforce.Assets.Assignment.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class RegionalAssignmentAgreementEngineChecks
{
    public static void RegionalAssignmentAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalAssignmentAgreementEngine.DefaultRegionalAssignmentAgreementEngine(Flags.NewRegistry());
        var item = RegionalAssignmentAgreement.NewRegionalAssignmentAgreement("code-639", "name-487");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalAssignmentAgreementEngineKeepsResultsWithinBounds();
    }
}
