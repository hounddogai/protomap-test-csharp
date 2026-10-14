namespace Acme.Workforce.Contractors.Tests.Offboarding.Batch;

using Acme.Workforce.Contractors.Offboarding.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class OffboardingRuleChecks
{
    public static void OffboardingRuleStartsInRequested()
    {
        var item = OffboardingRule.NewOffboardingRule("code-927", "label-508");
        Check.Equal(item.Status, OffboardingRuleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingRuleMovesToGranted()
    {
        var item = OffboardingRule.NewOffboardingRule("code-927", "label-508");
        try
        {
            item.MoveTo(OffboardingRuleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingRuleStatus.Granted, "status");
    }

    public static void OffboardingRuleRejectsMovesFromExpired()
    {
        var item = OffboardingRule.NewOffboardingRule("code-927", "label-508");
        item.Status = OffboardingRuleStatus.Expired;
        Check.Fails(() => item.MoveTo(OffboardingRuleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        OffboardingRuleStartsInRequested();
        OffboardingRuleMovesToGranted();
        OffboardingRuleRejectsMovesFromExpired();
    }
}
