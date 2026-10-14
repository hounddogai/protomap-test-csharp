namespace Acme.Workforce.Orgdesign.Tests.Headcount.Entry;

using Acme.Workforce.Orgdesign.Headcount.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualHeadcountRuleChecks
{
    public static void ManualHeadcountRuleStartsInRequested()
    {
        var item = ManualHeadcountRule.NewManualHeadcountRule("code-328", "name-997");
        Check.Equal(item.Status, ManualHeadcountRuleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualHeadcountRuleMovesToGranted()
    {
        var item = ManualHeadcountRule.NewManualHeadcountRule("code-328", "name-997");
        try
        {
            item.MoveTo(ManualHeadcountRuleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualHeadcountRuleStatus.Granted, "status");
    }

    public static void ManualHeadcountRuleRejectsMovesFromExpired()
    {
        var item = ManualHeadcountRule.NewManualHeadcountRule("code-328", "name-997");
        item.Status = ManualHeadcountRuleStatus.Expired;
        Check.Fails(() => item.MoveTo(ManualHeadcountRuleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ManualHeadcountRuleStartsInRequested();
        ManualHeadcountRuleMovesToGranted();
        ManualHeadcountRuleRejectsMovesFromExpired();
    }
}
