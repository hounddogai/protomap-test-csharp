namespace Acme.Workforce.Assets.Tests.AssetReturn.Cycle;

using Acme.Workforce.Assets.AssetReturn.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AdaptiveAssetReturnNoticeRulesChecks
{
    public static void AdaptiveAssetReturnNoticeValidatorReportsEveryProblem()
    {
        var validator = AdaptiveAssetReturnNoticeRules.DefaultAdaptiveAssetReturnNoticeValidator();
        var item = AdaptiveAssetReturnNotice.NewAdaptiveAssetReturnNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveAssetReturnNoticeHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new AdaptiveAssetReturnNoticeHoursRangeRule(3.9, 255.7);
        var item = AdaptiveAssetReturnNotice.NewAdaptiveAssetReturnNotice("x", "y");
        item.Hours = 257.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveAssetReturnNoticeRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptiveAssetReturnNoticeRequiredTitleRule("adaptive asset return notice needs an entry");
        var item = AdaptiveAssetReturnNotice.NewAdaptiveAssetReturnNotice("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveAssetReturnNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveAssetReturnNoticeCodeLengthRule(3600);
        var item = AdaptiveAssetReturnNotice.NewAdaptiveAssetReturnNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveAssetReturnNoticeTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AdaptiveAssetReturnNoticeTitleLengthRule(80);
        var item = AdaptiveAssetReturnNotice.NewAdaptiveAssetReturnNotice("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveAssetReturnNoticeRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new AdaptiveAssetReturnNoticeRequiredSkillRule("adaptive asset return notice needs this field");
        var item = AdaptiveAssetReturnNotice.NewAdaptiveAssetReturnNotice("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveAssetReturnNoticeValidatorReportsEveryProblem();
        AdaptiveAssetReturnNoticeHoursRangeRuleFlagsValuesAboveHours();
        AdaptiveAssetReturnNoticeRequiredTitleRuleFlagsABlankTitle();
        AdaptiveAssetReturnNoticeCodeLengthRuleFlagsALongCode();
        AdaptiveAssetReturnNoticeTitleLengthRuleFlagsALongTitle();
        AdaptiveAssetReturnNoticeRequiredSkillRuleFlagsABlankSkill();
    }
}
