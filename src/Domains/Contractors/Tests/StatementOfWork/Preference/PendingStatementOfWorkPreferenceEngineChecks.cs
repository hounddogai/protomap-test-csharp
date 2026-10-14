namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PendingStatementOfWorkPreferenceEngineChecks
{
    public static void PendingStatementOfWorkPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = PendingStatementOfWorkPreferenceEngine.DefaultPendingStatementOfWorkPreferenceEngine(Flags.NewRegistry());
        var item = PendingStatementOfWorkPreference.NewPendingStatementOfWorkPreference("code-457", "name-678");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingStatementOfWorkPreferenceEngineKeepsResultsWithinBounds();
    }
}
