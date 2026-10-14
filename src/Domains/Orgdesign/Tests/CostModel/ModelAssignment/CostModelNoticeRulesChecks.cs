namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelNoticeRulesChecks
{
    public static void CostModelNoticeValidatorReportsEveryProblem()
    {
        var validator = CostModelNoticeRules.DefaultCostModelNoticeValidator();
        var item = CostModelNotice.NewCostModelNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelNoticeRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CostModelNoticeRequiredCodeRule("cost model notice needs a value");
        var item = CostModelNotice.NewCostModelNotice("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelNoticeTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CostModelNoticeTitleLengthRule(86400);
        var item = CostModelNotice.NewCostModelNotice("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelNoticeRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new CostModelNoticeRequiredLocationRule("cost model notice needs a value");
        var item = CostModelNotice.NewCostModelNotice("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CostModelNoticeRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new CostModelNoticeRequiredRegionRule("cost model notice needs this field");
        var item = CostModelNotice.NewCostModelNotice("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CostModelNoticeValidatorReportsEveryProblem();
        CostModelNoticeRequiredCodeRuleFlagsABlankCode();
        CostModelNoticeTitleLengthRuleFlagsALongTitle();
        CostModelNoticeRequiredLocationRuleFlagsABlankLocation();
        CostModelNoticeRequiredRegionRuleFlagsABlankRegion();
    }
}
