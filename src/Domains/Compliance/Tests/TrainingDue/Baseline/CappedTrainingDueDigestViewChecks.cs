namespace Acme.Workforce.Compliance.Tests.TrainingDue.Baseline;

using Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class CappedTrainingDueDigestViewChecks
{
    public static void CappedTrainingDueDigestViewShowsTheStatusInCapitals()
    {
        var item = CappedTrainingDueDigest.NewCappedTrainingDueDigest("code-974", "name-285");
        var view = CappedTrainingDueDigestView.ToCappedTrainingDueDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedTrainingDueDigestView.ToCappedTrainingDueDigestViews(new List<CappedTrainingDueDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedTrainingDueDigestViewShowsTheStatusInCapitals();
    }
}
