namespace Acme.Workforce.Talent.Tests.Nomination.NominationWindow;

using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Checks of nomination window code, run by the project's runner.</summary>
public static class CappedNominationWindowEngineChecks
{
    public static void CappedNominationWindowEngineKeepsResultsWithinBounds()
    {
        var engine = CappedNominationWindowEngine.DefaultCappedNominationWindowEngine();
        var item = CappedNominationWindow.NewCappedNominationWindow("code-65", "label-799");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedNominationWindowEngineKeepsResultsWithinBounds();
    }
}
