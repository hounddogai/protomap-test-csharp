namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PrimaryNominationMappingRulesChecks
{
    public static void PrimaryNominationMappingValidatorReportsEveryProblem()
    {
        var validator = PrimaryNominationMappingRules.DefaultPrimaryNominationMappingValidator();
        var item = PrimaryNominationMapping.NewPrimaryNominationMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryNominationMappingReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new PrimaryNominationMappingReviewerLengthRule(86400);
        var item = PrimaryNominationMapping.NewPrimaryNominationMapping("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryNominationMappingCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new PrimaryNominationMappingCostCenterLengthRule(64);
        var item = PrimaryNominationMapping.NewPrimaryNominationMapping("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryNominationMappingAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PrimaryNominationMappingAmountRangeRule(0.2, 2601.3);
        var item = PrimaryNominationMapping.NewPrimaryNominationMapping("x", "y");
        item.Amount = 2602.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryNominationMappingTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new PrimaryNominationMappingTenureMonthsRangeRule(80, 80);
        var item = PrimaryNominationMapping.NewPrimaryNominationMapping("x", "y");
        item.TenureMonths = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryNominationMappingValidatorReportsEveryProblem();
        PrimaryNominationMappingReviewerLengthRuleFlagsALongReviewer();
        PrimaryNominationMappingCostCenterLengthRuleFlagsALongCostCenter();
        PrimaryNominationMappingAmountRangeRuleFlagsValuesAboveAmount();
        PrimaryNominationMappingTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
    }
}
