namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PendingExceptionProposalEngineChecks
{
    public static void PendingExceptionProposalEngineKeepsResultsWithinBounds()
    {
        var engine = PendingExceptionProposalEngine.DefaultPendingExceptionProposalEngine();
        var item = PendingExceptionProposal.NewPendingExceptionProposal("code-498", "title-702");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingExceptionProposalEngineKeepsResultsWithinBounds();
    }
}
