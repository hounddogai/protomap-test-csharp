namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class CappedTrainingDueTargetViewChecks
{
    public static void CappedTrainingDueTargetViewShowsTheStatusInCapitals()
    {
        var item = CappedTrainingDueTarget.NewCappedTrainingDueTarget("code-545", "label-814");
        var view = CappedTrainingDueTargetView.ToCappedTrainingDueTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedTrainingDueTargetView.ToCappedTrainingDueTargetViews(new List<CappedTrainingDueTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedTrainingDueTargetViewShowsTheStatusInCapitals();
    }
}
