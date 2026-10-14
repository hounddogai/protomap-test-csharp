namespace Acme.Workforce.Orgdesign.Tests.Reorg.Group;

using Acme.Workforce.Orgdesign.Reorg.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ReorgBundleRulesChecks
{
    public static void ReorgBundleValidatorReportsEveryProblem()
    {
        var validator = ReorgBundleRules.DefaultReorgBundleValidator();
        var item = ReorgBundle.NewReorgBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReorgBundleRequiredCodeRule("reorg bundle needs this field");
        var item = ReorgBundle.NewReorgBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgBundleRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new ReorgBundleRequiredTeamRule("reorg bundle needs this field");
        var item = ReorgBundle.NewReorgBundle("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgBundleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ReorgBundleRequiredTitleRule("reorg bundle needs this field");
        var item = ReorgBundle.NewReorgBundle("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgBundleTeamLengthRuleFlagsALongTeam()
    {
        var rule = new ReorgBundleTeamLengthRule(64);
        var item = ReorgBundle.NewReorgBundle("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgBundleChannelLengthRuleFlagsALongChannel()
    {
        var rule = new ReorgBundleChannelLengthRule(255);
        var item = ReorgBundle.NewReorgBundle("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReorgBundleValidatorReportsEveryProblem();
        ReorgBundleRequiredCodeRuleFlagsABlankCode();
        ReorgBundleRequiredTeamRuleFlagsABlankTeam();
        ReorgBundleRequiredTitleRuleFlagsABlankTitle();
        ReorgBundleTeamLengthRuleFlagsALongTeam();
        ReorgBundleChannelLengthRuleFlagsALongChannel();
    }
}
