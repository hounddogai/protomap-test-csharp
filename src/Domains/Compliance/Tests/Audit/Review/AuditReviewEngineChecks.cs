namespace Acme.Workforce.Compliance.Tests.Audit.Review;

using Acme.Workforce.Compliance.Audit.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AuditReviewEngineChecks
{
    public static void AuditReviewEngineKeepsResultsWithinBounds()
    {
        var engine = AuditReviewEngine.DefaultAuditReviewEngine(Flags.NewRegistry());
        var item = AuditReview.NewAuditReview("code-216", "title-136");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditReviewEngineKeepsResultsWithinBounds();
    }
}
