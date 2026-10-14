namespace Acme.Workforce.Engagement.Tests.Recognition.Entry;

using Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PrimaryRecognitionPolicyChecks
{
    public static void PrimaryRecognitionPolicyStartsInDraft()
    {
        var item = PrimaryRecognitionPolicy.NewPrimaryRecognitionPolicy("code-132", "label-176");
        Check.Equal(item.Status, PrimaryRecognitionPolicyStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryRecognitionPolicyMovesToActive()
    {
        var item = PrimaryRecognitionPolicy.NewPrimaryRecognitionPolicy("code-132", "label-176");
        try
        {
            item.MoveTo(PrimaryRecognitionPolicyStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryRecognitionPolicyStatus.Active, "status");
    }

    public static void PrimaryRecognitionPolicyRejectsMovesFromClosed()
    {
        var item = PrimaryRecognitionPolicy.NewPrimaryRecognitionPolicy("code-132", "label-176");
        item.Status = PrimaryRecognitionPolicyStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryRecognitionPolicyStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryRecognitionPolicyStartsInDraft();
        PrimaryRecognitionPolicyMovesToActive();
        PrimaryRecognitionPolicyRejectsMovesFromClosed();
    }
}
