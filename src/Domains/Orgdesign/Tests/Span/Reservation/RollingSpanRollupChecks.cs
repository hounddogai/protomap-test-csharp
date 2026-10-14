namespace Acme.Workforce.Orgdesign.Tests.Span.Reservation;

using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class RollingSpanRollupChecks
{
    public static void RollingSpanRollupStartsInOpen()
    {
        var item = RollingSpanRollup.NewRollingSpanRollup("code-122", "name-453");
        Check.Equal(item.Status, RollingSpanRollupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingSpanRollupMovesToInReview()
    {
        var item = RollingSpanRollup.NewRollingSpanRollup("code-122", "name-453");
        try
        {
            item.MoveTo(RollingSpanRollupStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingSpanRollupStatus.InReview, "status");
    }

    public static void RollingSpanRollupRejectsMovesFromArchived()
    {
        var item = RollingSpanRollup.NewRollingSpanRollup("code-122", "name-453");
        item.Status = RollingSpanRollupStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingSpanRollupStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingSpanRollupStartsInOpen();
        RollingSpanRollupMovesToInReview();
        RollingSpanRollupRejectsMovesFromArchived();
    }
}
