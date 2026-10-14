namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SpanRecordChecks
{
    public static void SpanRecordStartsInPlanned()
    {
        var item = SpanRecord.NewSpanRecord("code-676", "label-557");
        Check.Equal(item.Status, SpanRecordStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanRecordMovesToCancelled()
    {
        var item = SpanRecord.NewSpanRecord("code-676", "label-557");
        try
        {
            item.MoveTo(SpanRecordStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanRecordStatus.Cancelled, "status");
    }

    public static void SpanRecordRejectsMovesFromCancelled()
    {
        var item = SpanRecord.NewSpanRecord("code-676", "label-557");
        item.Status = SpanRecordStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SpanRecordStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SpanRecordStartsInPlanned();
        SpanRecordMovesToCancelled();
        SpanRecordRejectsMovesFromCancelled();
    }
}
