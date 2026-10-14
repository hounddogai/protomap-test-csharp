namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class ManualTrainingDueNoticeChecks
{
    public static void ManualTrainingDueNoticeStartsInRequested()
    {
        var item = ManualTrainingDueNotice.NewManualTrainingDueNotice("code-776", "name-901");
        Check.Equal(item.Status, ManualTrainingDueNoticeStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualTrainingDueNoticeMovesToRevoked()
    {
        var item = ManualTrainingDueNotice.NewManualTrainingDueNotice("code-776", "name-901");
        try
        {
            item.MoveTo(ManualTrainingDueNoticeStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualTrainingDueNoticeStatus.Revoked, "status");
    }

    public static void ManualTrainingDueNoticeRejectsMovesFromExpired()
    {
        var item = ManualTrainingDueNotice.NewManualTrainingDueNotice("code-776", "name-901");
        item.Status = ManualTrainingDueNoticeStatus.Expired;
        Check.Fails(() => item.MoveTo(ManualTrainingDueNoticeStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ManualTrainingDueNoticeStartsInRequested();
        ManualTrainingDueNoticeMovesToRevoked();
        ManualTrainingDueNoticeRejectsMovesFromExpired();
    }
}
