namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LegacySpanAdjustmentChecks
{
    public static void LegacySpanAdjustmentStartsInOpen()
    {
        var item = LegacySpanAdjustment.NewLegacySpanAdjustment("code-325", "name-395");
        Check.Equal(item.Status, LegacySpanAdjustmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacySpanAdjustmentMovesToArchived()
    {
        var item = LegacySpanAdjustment.NewLegacySpanAdjustment("code-325", "name-395");
        try
        {
            item.MoveTo(LegacySpanAdjustmentStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacySpanAdjustmentStatus.Archived, "status");
    }

    public static void LegacySpanAdjustmentRejectsMovesFromArchived()
    {
        var item = LegacySpanAdjustment.NewLegacySpanAdjustment("code-325", "name-395");
        item.Status = LegacySpanAdjustmentStatus.Archived;
        Check.Fails(() => item.MoveTo(LegacySpanAdjustmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        LegacySpanAdjustmentStartsInOpen();
        LegacySpanAdjustmentMovesToArchived();
        LegacySpanAdjustmentRejectsMovesFromArchived();
    }
}
