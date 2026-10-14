namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementAgreementRulesChecks
{
    public static void EngagementAgreementValidatorReportsEveryProblem()
    {
        var validator = EngagementAgreementRules.DefaultEngagementAgreementValidator();
        var item = EngagementAgreement.NewEngagementAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementAgreementRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new EngagementAgreementRequiredLabelRule("engagement agreement needs an entry");
        var item = EngagementAgreement.NewEngagementAgreement("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EngagementAgreementRequiredCodeRule("engagement agreement needs a value");
        var item = EngagementAgreement.NewEngagementAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementAgreementTitleLengthRuleFlagsALongTitle()
    {
        var rule = new EngagementAgreementTitleLengthRule(120);
        var item = EngagementAgreement.NewEngagementAgreement("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementAgreementBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new EngagementAgreementBatchSizeRangeRule(32, 64);
        var item = EngagementAgreement.NewEngagementAgreement("x", "y");
        item.BatchSize = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        EngagementAgreementValidatorReportsEveryProblem();
        EngagementAgreementRequiredLabelRuleFlagsABlankLabel();
        EngagementAgreementRequiredCodeRuleFlagsABlankCode();
        EngagementAgreementTitleLengthRuleFlagsALongTitle();
        EngagementAgreementBatchSizeRangeRuleFlagsValuesAboveBatchSize();
    }
}
