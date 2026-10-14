namespace Acme.Workforce.Workplace.Tests.AccessBadge.Profile;

using Acme.Workforce.Workplace.AccessBadge.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AccessBadgeRuleChecks
{
    public static void AccessBadgeRuleStartsInRequested()
    {
        var item = AccessBadgeRule.NewAccessBadgeRule("code-585", "label-661");
        Check.Equal(item.Status, AccessBadgeRuleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeRuleMovesToRevoked()
    {
        var item = AccessBadgeRule.NewAccessBadgeRule("code-585", "label-661");
        try
        {
            item.MoveTo(AccessBadgeRuleStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeRuleStatus.Revoked, "status");
    }

    public static void AccessBadgeRuleRejectsMovesFromExpired()
    {
        var item = AccessBadgeRule.NewAccessBadgeRule("code-585", "label-661");
        item.Status = AccessBadgeRuleStatus.Expired;
        Check.Fails(() => item.MoveTo(AccessBadgeRuleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AccessBadgeRuleStartsInRequested();
        AccessBadgeRuleMovesToRevoked();
        AccessBadgeRuleRejectsMovesFromExpired();
    }
}
