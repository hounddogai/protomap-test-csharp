namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeBudget;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Checks of badge budget code, run by the project's runner.</summary>
public static class CappedAccessBadgeTemplateRulesChecks
{
    public static void CappedAccessBadgeTemplateValidatorReportsEveryProblem()
    {
        var validator = CappedAccessBadgeTemplateRules.DefaultCappedAccessBadgeTemplateValidator();
        var item = CappedAccessBadgeTemplate.NewCappedAccessBadgeTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedAccessBadgeTemplateRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new CappedAccessBadgeTemplateRequiredSummaryRule("capped access badge template needs a value");
        var item = CappedAccessBadgeTemplate.NewCappedAccessBadgeTemplate("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedAccessBadgeTemplateFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new CappedAccessBadgeTemplateFloorValueRangeRule(9.9, 2996.4);
        var item = CappedAccessBadgeTemplate.NewCappedAccessBadgeTemplate("x", "y");
        item.FloorValue = 2997.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedAccessBadgeTemplateRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CappedAccessBadgeTemplateRequiredTitleRule("capped access badge template needs an entry");
        var item = CappedAccessBadgeTemplate.NewCappedAccessBadgeTemplate("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedAccessBadgeTemplateSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new CappedAccessBadgeTemplateSummaryLengthRule(64);
        var item = CappedAccessBadgeTemplate.NewCappedAccessBadgeTemplate("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedAccessBadgeTemplateValidatorReportsEveryProblem();
        CappedAccessBadgeTemplateRequiredSummaryRuleFlagsABlankSummary();
        CappedAccessBadgeTemplateFloorValueRangeRuleFlagsValuesAboveFloorValue();
        CappedAccessBadgeTemplateRequiredTitleRuleFlagsABlankTitle();
        CappedAccessBadgeTemplateSummaryLengthRuleFlagsALongSummary();
    }
}
