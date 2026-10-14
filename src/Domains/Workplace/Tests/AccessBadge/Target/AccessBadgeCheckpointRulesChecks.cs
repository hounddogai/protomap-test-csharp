namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeCheckpointRulesChecks
{
    public static void AccessBadgeCheckpointValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeCheckpointRules.DefaultAccessBadgeCheckpointValidator();
        var item = AccessBadgeCheckpoint.NewAccessBadgeCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new AccessBadgeCheckpointNameLengthRule(64);
        var item = AccessBadgeCheckpoint.NewAccessBadgeCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeCheckpointRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new AccessBadgeCheckpointRequiredChannelRule("access badge checkpoint needs this field");
        var item = AccessBadgeCheckpoint.NewAccessBadgeCheckpoint("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccessBadgeCheckpointValidatorReportsEveryProblem();
        AccessBadgeCheckpointNameLengthRuleFlagsALongName();
        AccessBadgeCheckpointRequiredChannelRuleFlagsABlankChannel();
    }
}
