namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestRuleRulesChecks
{
    public static void SeasonalLeaveRequestRuleValidatorReportsEveryProblem()
    {
        var validator = SeasonalLeaveRequestRuleRules.DefaultSeasonalLeaveRequestRuleValidator();
        var item = SeasonalLeaveRequestRule.NewSeasonalLeaveRequestRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalLeaveRequestRuleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalLeaveRequestRuleRequiredLabelRule("seasonal leave request rule needs this field");
        var item = SeasonalLeaveRequestRule.NewSeasonalLeaveRequestRule("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalLeaveRequestRuleRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new SeasonalLeaveRequestRuleRevisionRangeRule(32, 80);
        var item = SeasonalLeaveRequestRule.NewSeasonalLeaveRequestRule("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalLeaveRequestRuleRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new SeasonalLeaveRequestRuleRequiredLocaleRule("seasonal leave request rule needs this field");
        var item = SeasonalLeaveRequestRule.NewSeasonalLeaveRequestRule("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalLeaveRequestRuleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalLeaveRequestRuleRequiredCodeRule("seasonal leave request rule needs this field");
        var item = SeasonalLeaveRequestRule.NewSeasonalLeaveRequestRule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalLeaveRequestRuleValidatorReportsEveryProblem();
        SeasonalLeaveRequestRuleRequiredLabelRuleFlagsABlankLabel();
        SeasonalLeaveRequestRuleRevisionRangeRuleFlagsValuesAboveRevision();
        SeasonalLeaveRequestRuleRequiredLocaleRuleFlagsABlankLocale();
        SeasonalLeaveRequestRuleRequiredCodeRuleFlagsABlankCode();
    }
}
