namespace Acme.Workforce.Engagement.Tests.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RecognitionTemplateChecks
{
    public static void RecognitionTemplateStartsInProposed()
    {
        var item = RecognitionTemplate.NewRecognitionTemplate("code-894", "name-483");
        Check.Equal(item.Status, RecognitionTemplateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionTemplateMovesToApproved()
    {
        var item = RecognitionTemplate.NewRecognitionTemplate("code-894", "name-483");
        try
        {
            item.MoveTo(RecognitionTemplateStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionTemplateStatus.Approved, "status");
    }

    public static void RecognitionTemplateRejectsMovesFromWithdrawn()
    {
        var item = RecognitionTemplate.NewRecognitionTemplate("code-894", "name-483");
        item.Status = RecognitionTemplateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RecognitionTemplateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RecognitionTemplateStartsInProposed();
        RecognitionTemplateMovesToApproved();
        RecognitionTemplateRejectsMovesFromWithdrawn();
    }
}
