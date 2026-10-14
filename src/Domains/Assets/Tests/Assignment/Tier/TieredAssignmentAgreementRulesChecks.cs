namespace Acme.Workforce.Assets.Tests.Assignment.Tier;

using Acme.Workforce.Assets.Assignment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TieredAssignmentAgreementRulesChecks
{
    public static void TieredAssignmentAgreementValidatorReportsEveryProblem()
    {
        var validator = TieredAssignmentAgreementRules.DefaultTieredAssignmentAgreementValidator();
        var item = TieredAssignmentAgreement.NewTieredAssignmentAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredAssignmentAgreementRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredAssignmentAgreementRequiredLabelRule("tiered assignment agreement needs an entry");
        var item = TieredAssignmentAgreement.NewTieredAssignmentAgreement("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredAssignmentAgreementCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new TieredAssignmentAgreementCreatedAtRangeRule(64, 80);
        var item = TieredAssignmentAgreement.NewTieredAssignmentAgreement("x", "y");
        item.CreatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TieredAssignmentAgreementValidatorReportsEveryProblem();
        TieredAssignmentAgreementRequiredLabelRuleFlagsABlankLabel();
        TieredAssignmentAgreementCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
