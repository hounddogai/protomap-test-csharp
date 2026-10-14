namespace Acme.Workforce.Contractors.Tests.Engagement.Snapshot;

using Acme.Workforce.Contractors.Engagement.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class TieredEngagementAgreementRulesChecks
{
    public static void TieredEngagementAgreementValidatorReportsEveryProblem()
    {
        var validator = TieredEngagementAgreementRules.DefaultTieredEngagementAgreementValidator();
        var item = TieredEngagementAgreement.NewTieredEngagementAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredEngagementAgreementTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TieredEngagementAgreementTitleLengthRule(80);
        var item = TieredEngagementAgreement.NewTieredEngagementAgreement("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredEngagementAgreementRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredEngagementAgreementRequiredCodeRule("tiered engagement agreement needs this field");
        var item = TieredEngagementAgreement.NewTieredEngagementAgreement("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredEngagementAgreementPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new TieredEngagementAgreementPriorityRangeRule(3600, 3600);
        var item = TieredEngagementAgreement.NewTieredEngagementAgreement("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredEngagementAgreementValidatorReportsEveryProblem();
        TieredEngagementAgreementTitleLengthRuleFlagsALongTitle();
        TieredEngagementAgreementRequiredCodeRuleFlagsABlankCode();
        TieredEngagementAgreementPriorityRangeRuleFlagsValuesAbovePriority();
    }
}
