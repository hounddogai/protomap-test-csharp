namespace Acme.Workforce.Compensation.Tests.EquityGrant.Schedule;

using Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class EquityGrantSegmentChecks
{
    public static void EquityGrantSegmentStartsInNew()
    {
        var item = EquityGrantSegment.NewEquityGrantSegment("code-966", "name-313");
        Check.Equal(item.Status, EquityGrantSegmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantSegmentMovesToQueued()
    {
        var item = EquityGrantSegment.NewEquityGrantSegment("code-966", "name-313");
        try
        {
            item.MoveTo(EquityGrantSegmentStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantSegmentStatus.Queued, "status");
    }

    public static void EquityGrantSegmentRejectsMovesFromFailed()
    {
        var item = EquityGrantSegment.NewEquityGrantSegment("code-966", "name-313");
        item.Status = EquityGrantSegmentStatus.Failed;
        Check.Fails(() => item.MoveTo(EquityGrantSegmentStatus.New), "move");
    }

    public static void RunAll()
    {
        EquityGrantSegmentStartsInNew();
        EquityGrantSegmentMovesToQueued();
        EquityGrantSegmentRejectsMovesFromFailed();
    }
}
