namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TownHallNoticeEngineChecks
{
    public static void TownHallNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallNoticeEngine.DefaultTownHallNoticeEngine(Flags.NewRegistry());
        var item = TownHallNotice.NewTownHallNotice("code-293", "name-321");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallNoticeEngineKeepsResultsWithinBounds();
    }
}
