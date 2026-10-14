namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StrictPoolRuleChecks
{
    public static void StrictPoolRuleStartsInDraft()
    {
        var item = StrictPoolRule.NewStrictPoolRule("code-155", "title-975");
        Check.Equal(item.Status, StrictPoolRuleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPoolRuleMovesToActive()
    {
        var item = StrictPoolRule.NewStrictPoolRule("code-155", "title-975");
        try
        {
            item.MoveTo(StrictPoolRuleStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPoolRuleStatus.Active, "status");
    }

    public static void StrictPoolRuleRejectsMovesFromClosed()
    {
        var item = StrictPoolRule.NewStrictPoolRule("code-155", "title-975");
        item.Status = StrictPoolRuleStatus.Closed;
        Check.Fails(() => item.MoveTo(StrictPoolRuleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StrictPoolRuleStartsInDraft();
        StrictPoolRuleMovesToActive();
        StrictPoolRuleRejectsMovesFromClosed();
    }
}
