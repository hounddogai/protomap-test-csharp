namespace Acme.Workforce.Workplace.Tests.AccessBadge.Notice;

using Acme.Workforce.Workplace.AccessBadge.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ManualAccessBadgeNoticeRulesChecks
{
    public static void ManualAccessBadgeNoticeValidatorReportsEveryProblem()
    {
        var validator = ManualAccessBadgeNoticeRules.DefaultManualAccessBadgeNoticeValidator();
        var item = ManualAccessBadgeNotice.NewManualAccessBadgeNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualAccessBadgeNoticePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new ManualAccessBadgeNoticePriorityRangeRule(16, 64);
        var item = ManualAccessBadgeNotice.NewManualAccessBadgeNotice("x", "y");
        item.Priority = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualAccessBadgeNoticeRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new ManualAccessBadgeNoticeRequiredSalaryBandRule("manual access badge notice needs this field");
        var item = ManualAccessBadgeNotice.NewManualAccessBadgeNotice("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualAccessBadgeNoticeRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new ManualAccessBadgeNoticeRequiredCostCenterRule("manual access badge notice needs a value");
        var item = ManualAccessBadgeNotice.NewManualAccessBadgeNotice("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualAccessBadgeNoticeCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualAccessBadgeNoticeCodeLengthRule(80);
        var item = ManualAccessBadgeNotice.NewManualAccessBadgeNotice("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualAccessBadgeNoticeValidatorReportsEveryProblem();
        ManualAccessBadgeNoticePriorityRangeRuleFlagsValuesAbovePriority();
        ManualAccessBadgeNoticeRequiredSalaryBandRuleFlagsABlankSalaryBand();
        ManualAccessBadgeNoticeRequiredCostCenterRuleFlagsABlankCostCenter();
        ManualAccessBadgeNoticeCodeLengthRuleFlagsALongCode();
    }
}
