namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueDigestChecks
{
    public static void TrainingDueDigestStartsInRequested()
    {
        var item = TrainingDueDigest.NewTrainingDueDigest("code-281", "label-154");
        Check.Equal(item.Status, TrainingDueDigestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueDigestMovesToGranted()
    {
        var item = TrainingDueDigest.NewTrainingDueDigest("code-281", "label-154");
        try
        {
            item.MoveTo(TrainingDueDigestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueDigestStatus.Granted, "status");
    }

    public static void TrainingDueDigestRejectsMovesFromExpired()
    {
        var item = TrainingDueDigest.NewTrainingDueDigest("code-281", "label-154");
        item.Status = TrainingDueDigestStatus.Expired;
        Check.Fails(() => item.MoveTo(TrainingDueDigestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TrainingDueDigestStartsInRequested();
        TrainingDueDigestMovesToGranted();
        TrainingDueDigestRejectsMovesFromExpired();
    }
}
