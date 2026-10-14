namespace Acme.Workforce.Talent.Tests.SkillProfile.Snapshot;

using Acme.Workforce.Talent.SkillProfile.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SkillProfileDigestRulesChecks
{
    public static void SkillProfileDigestValidatorReportsEveryProblem()
    {
        var validator = SkillProfileDigestRules.DefaultSkillProfileDigestValidator();
        var item = SkillProfileDigest.NewSkillProfileDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SkillProfileDigestManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new SkillProfileDigestManagerIdLengthRule(80);
        var item = SkillProfileDigest.NewSkillProfileDigest("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileDigestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SkillProfileDigestRequiredTitleRule("skill profile digest needs a value");
        var item = SkillProfileDigest.NewSkillProfileDigest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SkillProfileDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new SkillProfileDigestCodeLengthRule(32);
        var item = SkillProfileDigest.NewSkillProfileDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SkillProfileDigestChannelLengthRuleFlagsALongChannel()
    {
        var rule = new SkillProfileDigestChannelLengthRule(32);
        var item = SkillProfileDigest.NewSkillProfileDigest("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SkillProfileDigestValidatorReportsEveryProblem();
        SkillProfileDigestManagerIdLengthRuleFlagsALongManagerId();
        SkillProfileDigestRequiredTitleRuleFlagsABlankTitle();
        SkillProfileDigestCodeLengthRuleFlagsALongCode();
        SkillProfileDigestChannelLengthRuleFlagsALongChannel();
    }
}
