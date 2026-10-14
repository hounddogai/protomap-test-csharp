namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LenientLeaveRequestAgreementEngineChecks
{
    public static void LenientLeaveRequestAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = LenientLeaveRequestAgreementEngine.DefaultLenientLeaveRequestAgreementEngine(Flags.NewRegistry());
        var item = LenientLeaveRequestAgreement.NewLenientLeaveRequestAgreement("code-193", "name-871");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientLeaveRequestAgreementEngineKeepsResultsWithinBounds();
    }
}
