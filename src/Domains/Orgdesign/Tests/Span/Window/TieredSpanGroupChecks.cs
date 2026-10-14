namespace Acme.Workforce.Orgdesign.Tests.Span.Window;

using Acme.Workforce.Orgdesign.Span.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredSpanGroupChecks
{
    public static void TieredSpanGroupStartsInRequested()
    {
        var item = TieredSpanGroup.NewTieredSpanGroup("code-839", "title-177");
        Check.Equal(item.Status, TieredSpanGroupStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredSpanGroupMovesToRevoked()
    {
        var item = TieredSpanGroup.NewTieredSpanGroup("code-839", "title-177");
        try
        {
            item.MoveTo(TieredSpanGroupStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredSpanGroupStatus.Revoked, "status");
    }

    public static void TieredSpanGroupRejectsMovesFromExpired()
    {
        var item = TieredSpanGroup.NewTieredSpanGroup("code-839", "title-177");
        item.Status = TieredSpanGroupStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredSpanGroupStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredSpanGroupStartsInRequested();
        TieredSpanGroupMovesToRevoked();
        TieredSpanGroupRejectsMovesFromExpired();
    }
}
