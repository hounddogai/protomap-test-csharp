namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class LegacyTrainingDueVarianceViewChecks
{
    public static void LegacyTrainingDueVarianceViewShowsTheStatusInCapitals()
    {
        var item = LegacyTrainingDueVariance.NewLegacyTrainingDueVariance("code-870", "label-513");
        var view = LegacyTrainingDueVarianceView.ToLegacyTrainingDueVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyTrainingDueVarianceView.ToLegacyTrainingDueVarianceViews(new List<LegacyTrainingDueVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyTrainingDueVarianceViewShowsTheStatusInCapitals();
    }
}
