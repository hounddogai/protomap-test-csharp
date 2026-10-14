namespace Acme.Workforce.Orgdesign.Tests.Span.SpanReview;

using Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>Checks of span review code, run by the project's runner.</summary>
public static class ManualSpanPlanChecks
{
    public static void ManualSpanPlanStartsInCreated()
    {
        var item = ManualSpanPlan.NewManualSpanPlan("code-511", "name-23");
        Check.Equal(item.Status, ManualSpanPlanStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualSpanPlanMovesToPublished()
    {
        var item = ManualSpanPlan.NewManualSpanPlan("code-511", "name-23");
        try
        {
            item.MoveTo(ManualSpanPlanStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualSpanPlanStatus.Published, "status");
    }

    public static void ManualSpanPlanRejectsMovesFromRetired()
    {
        var item = ManualSpanPlan.NewManualSpanPlan("code-511", "name-23");
        item.Status = ManualSpanPlanStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualSpanPlanStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualSpanPlanStartsInCreated();
        ManualSpanPlanMovesToPublished();
        ManualSpanPlanRejectsMovesFromRetired();
    }
}
