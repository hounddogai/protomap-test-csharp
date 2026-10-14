namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueDraft;

using Acme.Workforce.Compliance.TrainingDue.DueDraft;

/// <summary>Checks of due draft code, run by the project's runner.</summary>
public static class LegacyTrainingDueSegmentChecks
{
    public static void LegacyTrainingDueSegmentStartsInCreated()
    {
        var item = LegacyTrainingDueSegment.NewLegacyTrainingDueSegment("code-822", "label-968");
        Check.Equal(item.Status, LegacyTrainingDueSegmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTrainingDueSegmentMovesToPublished()
    {
        var item = LegacyTrainingDueSegment.NewLegacyTrainingDueSegment("code-822", "label-968");
        try
        {
            item.MoveTo(LegacyTrainingDueSegmentStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTrainingDueSegmentStatus.Published, "status");
    }

    public static void LegacyTrainingDueSegmentRejectsMovesFromRetired()
    {
        var item = LegacyTrainingDueSegment.NewLegacyTrainingDueSegment("code-822", "label-968");
        item.Status = LegacyTrainingDueSegmentStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyTrainingDueSegmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyTrainingDueSegmentStartsInCreated();
        LegacyTrainingDueSegmentMovesToPublished();
        LegacyTrainingDueSegmentRejectsMovesFromRetired();
    }
}
