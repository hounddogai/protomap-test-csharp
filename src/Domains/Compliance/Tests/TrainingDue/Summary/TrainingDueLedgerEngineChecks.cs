namespace Acme.Workforce.Compliance.Tests.TrainingDue.Summary;

using Acme.Workforce.Compliance.TrainingDue.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TrainingDueLedgerEngineChecks
{
    public static void TrainingDueLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueLedgerEngine.DefaultTrainingDueLedgerEngine(Flags.NewRegistry());
        var item = TrainingDueLedger.NewTrainingDueLedger("code-371", "name-830");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueLedgerEngineKeepsResultsWithinBounds();
    }
}
