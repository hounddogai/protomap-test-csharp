namespace Acme.Workforce.Engagement.Tests.Recognition.Preference;

using Acme.Workforce.Engagement.Recognition.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RecognitionRuleChecks
{
    public static void RecognitionRuleStartsInRequested()
    {
        var item = RecognitionRule.NewRecognitionRule("code-801", "title-973");
        Check.Equal(item.Status, RecognitionRuleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionRuleMovesToRevoked()
    {
        var item = RecognitionRule.NewRecognitionRule("code-801", "title-973");
        try
        {
            item.MoveTo(RecognitionRuleStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionRuleStatus.Revoked, "status");
    }

    public static void RecognitionRuleRejectsMovesFromExpired()
    {
        var item = RecognitionRule.NewRecognitionRule("code-801", "title-973");
        item.Status = RecognitionRuleStatus.Expired;
        Check.Fails(() => item.MoveTo(RecognitionRuleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RecognitionRuleStartsInRequested();
        RecognitionRuleMovesToRevoked();
        RecognitionRuleRejectsMovesFromExpired();
    }
}
