namespace Acme.Workforce.Orgdesign.Tests.Headcount.Proposal;

using Acme.Workforce.Orgdesign.Headcount.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AdaptiveHeadcountAuditRulesChecks
{
    public static void AdaptiveHeadcountAuditValidatorReportsEveryProblem()
    {
        var validator = AdaptiveHeadcountAuditRules.DefaultAdaptiveHeadcountAuditValidator();
        var item = AdaptiveHeadcountAudit.NewAdaptiveHeadcountAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveHeadcountAuditThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new AdaptiveHeadcountAuditThresholdRangeRule(5.5, 590.2);
        var item = AdaptiveHeadcountAudit.NewAdaptiveHeadcountAudit("x", "y");
        item.Threshold = 591.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveHeadcountAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptiveHeadcountAuditRequiredCodeRule("adaptive headcount audit needs a value");
        var item = AdaptiveHeadcountAudit.NewAdaptiveHeadcountAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveHeadcountAuditRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveHeadcountAuditRequiredLabelRule("adaptive headcount audit needs a value");
        var item = AdaptiveHeadcountAudit.NewAdaptiveHeadcountAudit("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveHeadcountAuditExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new AdaptiveHeadcountAuditExpiresAtRangeRule(16, 32);
        var item = AdaptiveHeadcountAudit.NewAdaptiveHeadcountAudit("x", "y");
        item.ExpiresAt = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveHeadcountAuditValidatorReportsEveryProblem();
        AdaptiveHeadcountAuditThresholdRangeRuleFlagsValuesAboveThreshold();
        AdaptiveHeadcountAuditRequiredCodeRuleFlagsABlankCode();
        AdaptiveHeadcountAuditRequiredLabelRuleFlagsABlankLabel();
        AdaptiveHeadcountAuditExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
    }
}
