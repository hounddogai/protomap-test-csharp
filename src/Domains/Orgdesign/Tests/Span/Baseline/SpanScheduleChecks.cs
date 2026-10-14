namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SpanScheduleChecks
{
    public static void SpanScheduleStartsInCreated()
    {
        var item = SpanSchedule.NewSpanSchedule("code-160", "name-882");
        Check.Equal(item.Status, SpanScheduleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanScheduleMovesToValidated()
    {
        var item = SpanSchedule.NewSpanSchedule("code-160", "name-882");
        try
        {
            item.MoveTo(SpanScheduleStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanScheduleStatus.Validated, "status");
    }

    public static void SpanScheduleRejectsMovesFromRetired()
    {
        var item = SpanSchedule.NewSpanSchedule("code-160", "name-882");
        item.Status = SpanScheduleStatus.Retired;
        Check.Fails(() => item.MoveTo(SpanScheduleStatus.Created), "move");
    }

    public static void RunAll()
    {
        SpanScheduleStartsInCreated();
        SpanScheduleMovesToValidated();
        SpanScheduleRejectsMovesFromRetired();
    }
}
