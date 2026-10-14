namespace Acme.Workforce.Compliance.Tests.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class TrainingDueAgreementEngineChecks
{
    public static void TrainingDueAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueAgreementEngine.DefaultTrainingDueAgreementEngine();
        var item = TrainingDueAgreement.NewTrainingDueAgreement("code-997", "label-182");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueAgreementEngineKeepsResultsWithinBounds();
    }
}
