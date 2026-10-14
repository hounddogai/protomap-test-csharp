namespace Acme.Workforce.Compliance.Tests.TrainingDue.Budget;

using Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LegacyTrainingDueBatchViewChecks
{
    public static void LegacyTrainingDueBatchViewShowsTheStatusInCapitals()
    {
        var item = LegacyTrainingDueBatch.NewLegacyTrainingDueBatch("code-872", "name-92");
        var view = LegacyTrainingDueBatchView.ToLegacyTrainingDueBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyTrainingDueBatchView.ToLegacyTrainingDueBatchViews(new List<LegacyTrainingDueBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyTrainingDueBatchViewShowsTheStatusInCapitals();
    }
}
