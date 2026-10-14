namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueDigestViewChecks
{
    public static void TrainingDueDigestViewShowsTheStatusInCapitals()
    {
        var item = TrainingDueDigest.NewTrainingDueDigest("code-281", "label-154");
        var view = TrainingDueDigestView.ToTrainingDueDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TrainingDueDigestView.ToTrainingDueDigestViews(new List<TrainingDueDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TrainingDueDigestViewShowsTheStatusInCapitals();
    }
}
