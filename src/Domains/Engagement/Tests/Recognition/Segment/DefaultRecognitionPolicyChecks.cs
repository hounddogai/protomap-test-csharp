namespace Acme.Workforce.Engagement.Tests.Recognition.Segment;

using Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DefaultRecognitionPolicyChecks
{
    public static void DefaultRecognitionPolicyStartsInCreated()
    {
        var item = DefaultRecognitionPolicy.NewDefaultRecognitionPolicy("code-947", "name-928");
        Check.Equal(item.Status, DefaultRecognitionPolicyStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultRecognitionPolicyMovesToValidated()
    {
        var item = DefaultRecognitionPolicy.NewDefaultRecognitionPolicy("code-947", "name-928");
        try
        {
            item.MoveTo(DefaultRecognitionPolicyStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultRecognitionPolicyStatus.Validated, "status");
    }

    public static void DefaultRecognitionPolicyRejectsMovesFromRetired()
    {
        var item = DefaultRecognitionPolicy.NewDefaultRecognitionPolicy("code-947", "name-928");
        item.Status = DefaultRecognitionPolicyStatus.Retired;
        Check.Fails(() => item.MoveTo(DefaultRecognitionPolicyStatus.Created), "move");
    }

    public static void RunAll()
    {
        DefaultRecognitionPolicyStartsInCreated();
        DefaultRecognitionPolicyMovesToValidated();
        DefaultRecognitionPolicyRejectsMovesFromRetired();
    }
}
