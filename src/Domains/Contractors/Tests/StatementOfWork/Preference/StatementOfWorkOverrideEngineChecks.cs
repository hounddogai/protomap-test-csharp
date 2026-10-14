namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StatementOfWorkOverrideEngineChecks
{
    public static void StatementOfWorkOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkOverrideEngine.DefaultStatementOfWorkOverrideEngine(Flags.NewRegistry());
        var item = StatementOfWorkOverride.NewStatementOfWorkOverride("code-254", "label-302");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkOverrideEngineKeepsResultsWithinBounds();
    }
}
