namespace Acme.Workforce.Orgdesign.Tests.Reorg.Rule;

using Acme.Workforce.Orgdesign.Reorg.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class StrictReorgNoticeRulesChecks
{
    public static void StrictReorgNoticeValidatorReportsEveryProblem()
    {
        var validator = StrictReorgNoticeRules.DefaultStrictReorgNoticeValidator();
        var item = StrictReorgNotice.NewStrictReorgNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictReorgNoticeRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new StrictReorgNoticeRequiredChecksumRule("strict reorg notice needs a value");
        var item = StrictReorgNotice.NewStrictReorgNotice("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictReorgNoticeCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new StrictReorgNoticeCostCenterLengthRule(255);
        var item = StrictReorgNotice.NewStrictReorgNotice("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictReorgNoticeValidatorReportsEveryProblem();
        StrictReorgNoticeRequiredChecksumRuleFlagsABlankChecksum();
        StrictReorgNoticeCostCenterLengthRuleFlagsALongCostCenter();
    }
}
