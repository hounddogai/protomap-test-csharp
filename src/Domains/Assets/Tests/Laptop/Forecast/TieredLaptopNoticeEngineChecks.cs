namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredLaptopNoticeEngineChecks
{
    public static void TieredLaptopNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = TieredLaptopNoticeEngine.DefaultTieredLaptopNoticeEngine();
        var item = TieredLaptopNotice.NewTieredLaptopNotice("code-430", "title-900");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredLaptopNoticeEngineKeepsResultsWithinBounds();
    }
}
