namespace Acme.Workforce.Payrollops.Tests.RetroPay.Allocation;

using Acme.Workforce.Payrollops.RetroPay.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RetroPayDigestEngineChecks
{
    public static void RetroPayDigestEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayDigestEngine.DefaultRetroPayDigestEngine();
        var item = RetroPayDigest.NewRetroPayDigest("code-97", "title-609");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayDigestEngineKeepsResultsWithinBounds();
    }
}
