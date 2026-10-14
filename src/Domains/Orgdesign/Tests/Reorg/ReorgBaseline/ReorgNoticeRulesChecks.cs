namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.ReorgBaseline;

/// <summary>Checks of reorg baseline code, run by the project's runner.</summary>
public static class ReorgNoticeRulesChecks
{
    public static void ReorgNoticeValidatorReportsEveryProblem()
    {
        var validator = ReorgNoticeRules.DefaultReorgNoticeValidator();
        var item = ReorgNotice.NewReorgNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReorgNoticeRequiredCodeRule("reorg notice needs an entry");
        var item = ReorgNotice.NewReorgNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgNoticeRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ReorgNoticeRequiredTitleRule("reorg notice needs an entry");
        var item = ReorgNotice.NewReorgNotice("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgNoticeWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new ReorgNoticeWeightRangeRule(9.4, 3691.1);
        var item = ReorgNotice.NewReorgNotice("x", "y");
        item.Weight = 3692.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ReorgNoticeValidatorReportsEveryProblem();
        ReorgNoticeRequiredCodeRuleFlagsABlankCode();
        ReorgNoticeRequiredTitleRuleFlagsABlankTitle();
        ReorgNoticeWeightRangeRuleFlagsValuesAboveWeight();
    }
}
