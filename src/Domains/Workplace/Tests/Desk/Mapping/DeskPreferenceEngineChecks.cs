namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskPreferenceEngineChecks
{
    public static void DeskPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = DeskPreferenceEngine.DefaultDeskPreferenceEngine(Flags.NewRegistry());
        var item = DeskPreference.NewDeskPreference("code-865", "label-796");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskPreferenceEngineKeepsResultsWithinBounds();
    }
}
