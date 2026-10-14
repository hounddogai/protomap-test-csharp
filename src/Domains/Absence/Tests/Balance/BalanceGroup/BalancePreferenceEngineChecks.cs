namespace Acme.Workforce.Absence.Tests.Balance.BalanceGroup;

using Acme.Workforce.Absence.Balance.BalanceGroup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of balance group code, run by the project's runner.</summary>
public static class BalancePreferenceEngineChecks
{
    public static void BalancePreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = BalancePreferenceEngine.DefaultBalancePreferenceEngine(Flags.NewRegistry());
        var item = BalancePreference.NewBalancePreference("code-393", "title-884");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalancePreferenceEngineKeepsResultsWithinBounds();
    }
}
