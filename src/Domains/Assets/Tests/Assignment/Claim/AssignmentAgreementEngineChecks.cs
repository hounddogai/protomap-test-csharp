namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AssignmentAgreementEngineChecks
{
    public static void AssignmentAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentAgreementEngine.DefaultAssignmentAgreementEngine();
        var item = AssignmentAgreement.NewAssignmentAgreement("code-324", "title-870");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentAgreementEngineKeepsResultsWithinBounds();
    }
}
